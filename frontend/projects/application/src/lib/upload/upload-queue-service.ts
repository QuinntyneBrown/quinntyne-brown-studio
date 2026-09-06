import { Injectable, OnDestroy, inject, signal } from '@angular/core';
import { UPLOAD_SERVICE } from '@qbs/api';
import { ApiError, UploadManifestEntry, UploadGrant } from '@qbs/domain';
import { IUploadQueueService } from '@qbs/api';
import { UploadResume } from './upload-resume';
import { UploadRow } from '@qbs/domain/models';
@Injectable()
export class UploadQueueService implements IUploadQueueService, OnDestroy {
  private readonly api = inject(UPLOAD_SERVICE);
  readonly rows = signal<UploadRow[]>([]);
  readonly busy = signal(false);
  readonly batchId = signal<string | null>(null);
  readonly warning = signal('');
  private destroyed = false;
  private readonly readers = new Map<Worker, () => void>();
  private canPersist = true;
  ngOnDestroy() {
    this.destroyed = true;
    for (const cancel of this.readers.values()) cancel();
  }
  private hash(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const worker = new Worker(new URL('./sha256.worker', import.meta.url), { type: 'module' });
      const close = () => {
        worker.terminate();
        this.readers.delete(worker);
      };
      this.readers.set(worker, () => {
        close();
        reject(new Error('File checking was cancelled. Reselect the files to resume.'));
      });
      worker.onmessage = ({
        data,
      }: MessageEvent<{
        error?: string;
        digest: string;
      }>) => {
        close();
        data.error ? reject(new Error(data.error)) : resolve(data.digest);
      };
      worker.onerror = () => {
        close();
        reject(new Error('Unable to read this file.'));
      };
      worker.postMessage(file);
    });
  }
  private saved(session: string): UploadResume | null {
    try {
      const text = localStorage.getItem('qbs-upload:' + session);
      if (!text) return null;
      const value: Partial<UploadResume> = JSON.parse(text);
      if (
        !value ||
        typeof value.id !== 'string' ||
        value.sessionId !== session ||
        !Array.isArray(value.manifest) ||
        !Array.isArray(value.files) ||
        !value.blocks ||
        typeof value.blocks !== 'object' ||
        Object.values(value.blocks).some(
          (blocks) => !Array.isArray(blocks) || blocks.some((block) => typeof block !== 'string'),
        )
      )
        throw new Error('Invalid resume data.');
      return value as UploadResume;
    } catch {
      this.warning.set('Saved resume information could not be read. A new batch was started.');
      return null;
    }
  }
  private persist(session: string, batch: UploadResume) {
    if (!this.canPersist) return;
    try {
      localStorage.setItem('qbs-upload:' + session, JSON.stringify(batch));
    } catch {
      this.canPersist = false;
      this.warning.set(
        'Resume information could not be saved in this browser. Keep this page open until transfer finishes.',
      );
    }
  }
  async start(session: string, files: FileList) {
    if (this.busy() || this.destroyed) return;
    if (!files.length || files.length > 1000) throw new Error('Select between 1 and 1,000 files.');
    this.busy.set(true);
    this.warning.set('');
    this.canPersist = true;
    const selected = Array.from(files);
    this.rows.set(
      selected.map((file) => ({ name: file.name, progress: 0, state: 'Checking file' })),
    );
    const localErrors = new Map<string, string>();
    try {
      const manifest: UploadManifestEntry[] = [];
      for (const [index, file] of selected.entries()) {
        if (this.destroyed) return;
        let sha256 = 'invalid';
        try {
          if (
            !/\.(jpg|jpeg|cr2|cr3|nef|arw|dng)$/i.test(file.name) ||
            file.size <= 0 ||
            file.size > 250000000
          )
            throw new Error('Unsupported format, empty file, or file exceeds 250 MB.');
          sha256 = await this.hash(file);
        } catch (error) {
          const message = error instanceof Error ? error.message : 'Unable to read this file.';
          localErrors.set(String(index), message);
          this.patch(index, { state: 'Rejected', error: message });
        }
        manifest.push({ clientFileId: String(index), name: file.name, size: file.size, sha256 });
      }
      if (this.destroyed) return;
      let batch = this.saved(session);
      if (!batch || JSON.stringify(batch.manifest) !== JSON.stringify(manifest)) {
        const created = await this.api.create(session, manifest);
        batch = {
          id: created.id,
          sessionId: created.sessionId,
          files: created.files,
          manifest,
          blocks: {},
        };
        this.persist(session, batch);
      }
      this.batchId.set(batch.id);
      const status = await this.api.status(batch.id);
      if (status.sessionId !== session)
        throw new Error('This saved batch belongs to another session. Select the files again.');
      batch.files = status.files;
      const active = batch;
      let next = 0;
      const transfer = async () => {
        while (next < selected.length && !this.destroyed) {
          const index = next++;
          const entry = active.files.find((file) => file.clientFileId === String(index));
          if (!entry || entry.rejection || !entry.photoId || localErrors.has(String(index))) {
            this.patch(index, {
              state: 'Rejected',
              error: localErrors.get(String(index)) ?? entry?.rejection ?? 'Unsupported file.',
            });
            continue;
          }
          try {
            if (entry.state && entry.state !== 'Uploading') {
              this.patch(index, {
                progress: 100,
                state: entry.state,
                error: entry.failure ?? undefined,
              });
              continue;
            }
            let grant = await this.api.renew(active.id, entry.photoId);
            const transferWithRenewal = async (
              operation: (current: UploadGrant) => Promise<void>,
            ) => {
              try {
                await operation(grant);
              } catch (error) {
                if (!(error instanceof ApiError) || error.kind !== 'forbidden') throw error;
                grant = await this.api.renew(active.id, entry.photoId!);
                await operation(grant);
              }
            };
            const file = selected[index];
            const blocks: string[] = [];
            const completed = active.blocks[entry.photoId] ?? [];
            for (
              let offset = 0, block = 0;
              offset < file.size;
              offset += 8 * 1024 * 1024, block++
            ) {
              if (this.destroyed) return;
              const id = btoa(String(block).padStart(8, '0'));
              blocks.push(id);
              if (!completed.includes(id)) {
                await transferWithRenewal((current) =>
                  this.api.block(current, id, file.slice(offset, offset + 8 * 1024 * 1024)),
                );
                completed.push(id);
                active.blocks[entry.photoId] = completed;
                this.persist(session, active);
              }
              this.patch(index, {
                progress: Math.round(
                  (Math.min(file.size, offset + 8 * 1024 * 1024) / file.size) * 100,
                ),
                state: 'Uploading',
              });
            }
            if (this.destroyed) return;
            await transferWithRenewal((current) => this.api.commit(current, blocks));
            const finalized = await this.api.complete(active.id, entry.photoId);
            this.patch(index, {
              progress: 100,
              state: finalized.state,
              error: finalized.failure ?? undefined,
            });
          } catch (error) {
            this.patch(index, {
              state: 'Interrupted',
              error:
                error instanceof Error
                  ? error.message
                  : 'Transfer failed. Reselect the files to resume.',
            });
          }
        }
      };
      await Promise.all(Array.from({ length: Math.min(4, selected.length) }, transfer));
    } catch (error) {
      this.rows.update((rows) =>
        rows.map((row) =>
          row.state === 'Checking file'
            ? {
                ...row,
                state: 'Interrupted',
                error: error instanceof Error ? error.message : 'Unable to start transfer.',
              }
            : row,
        ),
      );
      throw error;
    } finally {
      this.busy.set(false);
    }
  }
  async refreshStatus() {
    if (!this.batchId() || this.busy() || this.destroyed) return;
    const status = await this.api.status(this.batchId()!);
    for (const file of status.files) {
      if (file.state && file.state !== 'Uploading')
        this.patch(Number(file.clientFileId), {
          state: file.state,
          error: file.failure ?? file.rejection ?? undefined,
        });
    }
  }
  private patch(index: number, change: Partial<UploadRow>) {
    this.rows.update((rows) => rows.map((row, i) => (i === index ? { ...row, ...change } : row)));
  }
}
