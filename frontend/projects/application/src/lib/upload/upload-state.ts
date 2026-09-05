import { Injectable, inject, signal } from '@angular/core';
import { UPLOADS_API } from '@qbs/api';
@Injectable()
export class UploadState {
  private api = inject(UPLOADS_API);
  rows = signal<{ name: string; progress: number; state: string; error?: string }[]>([]);
  busy = signal(false);
  batchId = signal<string | null>(null);
  private hash(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const worker = new Worker(new URL('./sha256.worker', import.meta.url), { type: 'module' });
      worker.onmessage = ({ data }) => {
        worker.terminate();
        data.error ? reject(new Error(data.error)) : resolve(data.digest);
      };
      worker.onerror = () => {
        worker.terminate();
        reject(new Error('Unable to read this file.'));
      };
      worker.postMessage(file);
    });
  }
  async start(session: string, files: FileList) {
    if (files.length > 1000) throw new Error('Select at most 1,000 files.');
    this.busy.set(true);
    const selected = Array.from(files);
    this.rows.set(selected.map((f) => ({ name: f.name, progress: 0, state: 'Checking file' })));
    try {
      const manifests = [];
      for (let i = 0; i < selected.length; i++) {
        const file = selected[i];
        const sha256 = file.size > 250000000 ? 'invalid' : await this.hash(file);
        manifests.push({ clientFileId: String(i), name: file.name, size: file.size, sha256 });
      }
      const saved = localStorage.getItem('qbs-upload:' + session);
      let batch: any = saved ? JSON.parse(saved) : null;
      if (!batch || JSON.stringify(batch.manifest) !== JSON.stringify(manifests)) {
        batch = await this.api.send<any>('POST', 'admin/sessions/' + session + '/uploads', {
          files: manifests,
        });
        batch.manifest = manifests;
        batch.blocks = {};
        localStorage.setItem('qbs-upload:' + session, JSON.stringify(batch));
      }
      this.batchId.set(batch.id);
      let next = 0;
      const transfer = async () => {
        while (next < selected.length) {
          const index = next++;
          const entry = batch.files[index];
          if (entry.rejection || !entry.photoId) {
            this.patch(index, { state: 'Rejected', error: entry.rejection ?? 'Unsupported file.' });
            continue;
          }
          try {
            const status = await this.api.get<any>('admin/uploads/' + batch.id);
            const state = status.files.find((f: any) => f.photoId === entry.photoId)?.state;
            if (state && state !== 'Uploading') {
              this.patch(index, { progress: 100, state });
              continue;
            }
            const grant = await this.api.send<any>(
              'POST',
              `admin/uploads/${batch.id}/files/${entry.photoId}/renew`,
              {},
            );
            const file = selected[index];
            const blocks: string[] = [];
            const completed: string[] = batch.blocks[entry.photoId] ?? [];
            for (let offset = 0, n = 0; offset < file.size; offset += 8 * 1024 * 1024, n++) {
              const id = btoa(String(n).padStart(8, '0'));
              blocks.push(id);
              if (!completed.includes(id)) {
                await this.api.upload(grant.url, id, file.slice(offset, offset + 8 * 1024 * 1024));
                completed.push(id);
                batch.blocks[entry.photoId] = completed;
                localStorage.setItem('qbs-upload:' + session, JSON.stringify(batch));
              }
              this.patch(index, {
                progress: Math.round(
                  (Math.min(file.size, offset + 8 * 1024 * 1024) / file.size) * 100,
                ),
                state: 'Uploading',
              });
            }
            await this.api.upload(
              grant.url,
              undefined,
              '<?xml version="1.0" encoding="utf-8"?><BlockList>' +
                blocks.map((b) => '<Latest>' + b + '</Latest>').join('') +
                '</BlockList>',
            );
            const finalized = await this.api.send<any>(
              'POST',
              `admin/uploads/${batch.id}/files/${entry.photoId}/complete`,
              {},
            );
            this.patch(index, { progress: 100, state: finalized.state, error: finalized.failure });
          } catch (e) {
            this.patch(index, {
              state: 'Interrupted',
              error: e instanceof Error ? e.message : 'Transfer failed.',
            });
          }
        }
      };
      await Promise.all(Array.from({ length: Math.min(4, selected.length) }, transfer));
    } finally {
      this.busy.set(false);
    }
  }
  async refreshStatus() {
    if (!this.batchId() || this.busy()) return;
    const status = await this.api.get<any>('admin/uploads/' + this.batchId());
    status.files.forEach((file: any, index: number) => {
      if (file.state !== 'Uploading')
        this.patch(index, { state: file.state, error: file.failure ?? file.rejection });
    });
  }
  private patch(
    i: number,
    change: Partial<{ name: string; progress: number; state: string; error: string }>,
  ) {
    this.rows.update((rows) =>
      rows.map((row, index) => (index === i ? { ...row, ...change } : row)),
    );
  }
}
