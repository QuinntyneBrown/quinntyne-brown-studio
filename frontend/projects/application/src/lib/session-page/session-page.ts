import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { STUDIO_CLIENT } from '@qbs/api';
import { PhotoView } from '@qbs/domain';
import { PhotoGrid, Notice, Dialog, EmptyState } from '@qbs/components';
import { UploadState } from '../upload/upload-state';
@Component({
  selector: 'qbs-session-page',
  providers: [UploadState],
  imports: [FormsModule, PhotoGrid, Notice, Dialog, EmptyState],
  templateUrl: './session-page.html',
  styleUrl: './session-page.css',
})
export class SessionPage implements OnInit, OnDestroy {
  private api = inject(STUDIO_CLIENT);
  private route = inject(ActivatedRoute);
  id = this.route.snapshot.paramMap.get('id') ?? 'example-session';
  session = signal<any>(null);
  photos = signal<PhotoView[]>([]);
  people = signal<any[]>([]);
  selected = signal<string[]>([]);
  assignments: string[] = [];
  opened = signal<PhotoView | null>(null);
  message = signal('');
  error = signal(false);
  retention = signal<any>(null);
  analysis = signal<any>(null);
  nextCursor = signal<string | null>(null);
  upload = inject(UploadState);
  months = 12;
  extension = '';
  confirming = signal(false);
  private poll?: ReturnType<typeof setInterval>;
  private analysisId: string | null = null;
  private refreshing = false;
  async ngOnInit() {
    await this.load();
    this.poll = setInterval(() => void this.refresh(), 3000);
  }
  ngOnDestroy() {
    clearInterval(this.poll);
  }
  async load() {
    try {
      this.session.set(await this.api.get<any>('admin/sessions/' + this.id));
      this.assignments = this.session().clientIds ?? [];
      this.people.set(await this.api.get<any[]>('admin/clients'));
      this.retention.set(await this.api.get('admin/sessions/' + this.id + '/retention'));
      this.months = this.retention().months;
      await this.refresh();
    } catch (e) {
      this.fail(e);
    }
  }
  async refresh() {
    if (this.refreshing) return;
    this.refreshing = true;
    try {
      const count = Math.max(50, this.photos().length);
      let cursor: string | null = null;
      const items: PhotoView[] = [];
      do {
        const page: { photos: PhotoView[]; nextCursor: string | null } = await this.api.get<{
          photos: PhotoView[];
          nextCursor: string | null;
        }>('admin/sessions/' + this.id + '/photos' + (cursor ? '?cursor=' + cursor : ''));
        items.push(...page.photos);
        cursor = page.nextCursor;
      } while (cursor && items.length < count);
      this.photos.set(items);
      this.nextCursor.set(cursor);
      await this.upload.refreshStatus();
      if (this.analysisId)
        this.analysis.set(await this.api.get('admin/analysis/' + this.analysisId));
    } catch (e) {
      this.fail(e);
    } finally {
      this.refreshing = false;
    }
  }
  async more() {
    const page = await this.api.get<{ photos: PhotoView[]; nextCursor: string | null }>(
      'admin/sessions/' + this.id + '/photos?cursor=' + this.nextCursor(),
    );
    this.photos.update((p) => [...p, ...page.photos]);
    this.nextCursor.set(page.nextCursor);
  }
  toggle(id: string) {
    this.selected.update((ids) => (ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id]));
  }
  toggleClient(id: string) {
    this.assignments = this.assignments.includes(id)
      ? this.assignments.filter((x) => x !== id)
      : [...this.assignments, id];
  }
  async saveAssignments() {
    await this.action(async () => {
      this.session.set(
        await this.api.send('PUT', 'admin/sessions/' + this.id + '/clients', {
          clientIds: this.assignments,
          expectedVersion: this.session().version,
        }),
      );
    }, 'Gallery access saved.');
  }
  async files(event: Event) {
    const files = (event.target as HTMLInputElement).files;
    if (files)
      await this.action(async () => {
        await this.upload.start(this.id, files);
        this.session.set(await this.api.get('admin/sessions/' + this.id));
        this.retention.set(await this.api.get('admin/sessions/' + this.id + '/retention'));
        await this.refresh();
      }, 'Transfer finished. Ready previews appear below; interrupted files can be resumed.');
  }
  async analyze() {
    await this.action(async () => {
      const result = await this.api.send<any>('POST', 'admin/sessions/' + this.id + '/analysis', {
        photoIds: this.selected(),
      });
      this.analysisId = result.id;
      await this.refresh();
    }, 'AI review queued. You can continue reviewing photos.');
  }
  async retryPhoto(id: string) {
    await this.action(async () => {
      await this.api.send('POST', 'admin/photos/' + id + '/retry-preview', {});
      await this.refresh();
    }, 'Preview retry queued.');
  }
  async retryAnalysis() {
    await this.action(async () => {
      const result = await this.api.send<any>(
        'POST',
        'admin/analysis/' + this.analysisId + '/retry',
        {
          failedPhotoIds: this.analysis()
            .photos.filter((p: any) => p.state === 'Failed')
            .map((p: any) => p.photoId),
        },
      );
      this.analysisId = result.id;
      await this.refresh();
    }, 'Analysis retry queued.');
  }
  async extend() {
    await this.action(async () => {
      await this.api.send('PUT', 'admin/sessions/' + this.id + '/retention', {
        months: this.months,
        expiresAt: this.extension || null,
        expectedVersion: this.retention().version,
      });
      await this.load();
    }, 'Retention updated.');
  }
  async deletion() {
    await this.action(async () => {
      await this.api.send('POST', 'admin/sessions/' + this.id + '/photo-deletion', {
        impactRevision: this.retention().impactRevision,
        confirm: true,
      });
      this.confirming.set(false);
      await this.load();
    }, 'Photo deletion queued.');
  }
  async action(fn: () => Promise<unknown>, success: string) {
    try {
      await fn();
      this.error.set(false);
      this.message.set(success);
    } catch (e) {
      this.fail(e);
    }
  }
  fail(e: unknown) {
    this.error.set(true);
    this.message.set(e instanceof Error ? e.message : 'Unable to complete this action.');
  }
}
