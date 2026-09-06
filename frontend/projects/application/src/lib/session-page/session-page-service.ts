import { TORONTO_TIME_SERVICE } from '@qbs/api';
import { Injectable, inject, signal, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  SESSION_SERVICE,
  CLIENT_GALLERY_SERVICE,
  PHOTO_SERVICE,
  RETENTION_SERVICE,
  ANALYSIS_SERVICE,
} from '@qbs/api';
import {
  ApiError,
  PhotoView,
  StudioSession,
  ClientAccount,
  RetentionImpact,
  AnalysisBatch,
} from '@qbs/domain';
import { UPLOAD_QUEUE_SERVICE } from '@qbs/api';
import { ISessionPageService } from '@qbs/api';
@Injectable()
export class SessionPageService implements ISessionPageService, OnDestroy {
  readonly loading = signal(true);
  readonly loadFailed = signal(false);
  readonly photoLoadFailed = signal(false);
  readonly busy = signal(false);
  readonly moreBusy = signal(false);
  private destroyed = false;
  readonly time = inject(TORONTO_TIME_SERVICE);
  private sessions = inject(SESSION_SERVICE);
  private clients = inject(CLIENT_GALLERY_SERVICE);
  private photoService = inject(PHOTO_SERVICE);
  private retentions = inject(RETENTION_SERVICE);
  private analyses = inject(ANALYSIS_SERVICE);
  private route = inject(ActivatedRoute);
  id = this.route.snapshot.paramMap.get('id') ?? '';
  session = signal<StudioSession | null>(null);
  photos = signal<PhotoView[]>([]);
  people = signal<ClientAccount[]>([]);
  selected = signal<string[]>([]);
  assignments = signal<string[]>([]);
  opened = signal<PhotoView | null>(null);
  message = signal('');
  error = signal(false);
  retention = signal<RetentionImpact | null>(null);
  analysis = signal<AnalysisBatch | null>(null);
  nextCursor = signal<string | null>(null);
  upload = inject(UPLOAD_QUEUE_SERVICE);
  months = signal(12);
  extension = signal('');
  confirming = signal(false);
  private poll?: ReturnType<typeof setInterval>;
  private analysisId: string | null = null;
  private refreshing = false;
  async initialize() {
    await this.load();
    if (!this.destroyed) this.poll = setInterval(() => void this.refresh(), 3000);
  }
  ngOnDestroy() {
    this.destroyed = true;
    clearInterval(this.poll);
  }
  async load() {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.message.set('');
    try {
      this.session.set(await this.sessions.get(this.id));
      this.assignments.set(this.session()!.clientIds ?? []);
      this.people.set(await this.clients.clients());
      this.retention.set(await this.retentions.get(this.id));
      this.months.set(this.retention()!.months);
      await this.refresh();
    } catch (e) {
      this.loadFailed.set(true);
      this.fail(e);
    } finally {
      this.loading.set(false);
    }
  }
  async refresh() {
    if (this.refreshing || this.destroyed || this.moreBusy() || this.loadFailed()) return;
    this.refreshing = true;
    try {
      const count = Math.max(50, this.photos().length);
      let cursor: string | null = null;
      const items: PhotoView[] = [];
      const seen = new Set<string>();
      do {
        const page: {
          photos: PhotoView[];
          nextCursor: string | null;
        } = await this.photoService.list(this.id, cursor);
        if (page.nextCursor && seen.has(page.nextCursor))
          throw new Error('Unable to continue this photo collection. Retry loading photographs.');
        if (page.nextCursor) seen.add(page.nextCursor);
        items.push(...page.photos);
        cursor = page.nextCursor;
      } while (cursor && items.length < count);
      if (this.destroyed) return;
      this.photoLoadFailed.set(false);
      this.photos.set(items);
      this.nextCursor.set(cursor);
      await this.upload.refreshStatus();
      if (this.analysisId) this.analysis.set(await this.analyses.get(this.analysisId));
    } catch (e) {
      this.photoLoadFailed.set(true);
      this.fail(e);
    } finally {
      this.refreshing = false;
    }
  }
  async more() {
    if (!this.nextCursor() || this.moreBusy() || this.refreshing) return;
    this.moreBusy.set(true);
    try {
      const page = await this.photoService.list(this.id, this.nextCursor());
      if (page.nextCursor === this.nextCursor())
        throw new Error('Unable to continue this photo collection. Try again.');
      this.photos.update((photos) => [
        ...photos,
        ...page.photos.filter((photo) => !photos.some((existing) => existing.id === photo.id)),
      ]);
      this.nextCursor.set(page.nextCursor);
      this.photoLoadFailed.set(false);
    } catch (error) {
      this.fail(error);
    } finally {
      this.moreBusy.set(false);
    }
  }
  photoName(id: string) {
    return this.photos().find((photo) => photo.id === id)?.name ?? 'Unavailable photograph';
  }
  hasFailedAnalysis() {
    return this.analysis()?.photos.some((photo) => photo.state === 'Failed') ?? false;
  }
  toggle(id: string) {
    this.selected.update((ids) => (ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id]));
  }
  toggleClient(id: string) {
    this.assignments.set(
      this.assignments().includes(id)
        ? this.assignments().filter((x) => x !== id)
        : [...this.assignments(), id],
    );
  }
  async saveAssignments() {
    await this.action(async () => {
      this.session.set(
        await this.clients.assign(this.id, this.assignments(), this.session()!.version),
      );
    }, 'Gallery access saved.');
  }
  async files(event: Event) {
    const files = (event.target as HTMLInputElement).files;
    if (files)
      await this.action(async () => {
        await this.upload.start(this.id, files);
        this.session.set(await this.sessions.get(this.id));
        this.retention.set(await this.retentions.get(this.id));
        await this.refresh();
      }, 'Transfer finished. Ready previews appear below; interrupted files can be resumed.');
  }
  async analyze() {
    await this.action(async () => {
      const result = await this.analyses.start(this.id, this.selected());
      this.analysisId = result.id;
      await this.refresh();
    }, 'AI review queued. You can continue reviewing photos.');
  }
  async retryPhoto(id: string) {
    await this.action(async () => {
      await this.photoService.retry(id);
      await this.refresh();
    }, 'Preview retry queued.');
  }
  async retryAnalysis() {
    if (!this.analysisId || !this.hasFailedAnalysis()) return;
    await this.action(async () => {
      const result = await this.analyses.retry(
        this.analysisId!,
        this.analysis()!
          .photos.filter((p) => p.state === 'Failed')
          .map((p) => p.photoId),
      );
      this.analysisId = result.id;
      await this.refresh();
    }, 'Analysis retry queued.');
  }
  async extend() {
    await this.action(async () => {
      await this.retentions.extend(
        this.id,
        this.months(),
        this.extension() ? this.time.resolve(this.extension()) : null,
        this.retention()!.version,
      );
      await this.load();
    }, 'Retention updated.');
  }
  async deletion() {
    await this.action(async () => {
      try {
        await this.retentions.deletePhotos(this.id, this.retention()!.impactRevision);
      } catch (error) {
        if (error instanceof ApiError && error.kind === 'conflict') {
          this.confirming.set(false);
          this.retention.set(await this.retentions.get(this.id));
        }
        throw error;
      }
      this.confirming.set(false);
      await this.load();
    }, 'Photo deletion queued.');
  }
  async action(fn: () => Promise<unknown>, success: string) {
    if (this.busy()) return;
    this.busy.set(true);
    try {
      await fn();
      if (!this.loadFailed() && !this.photoLoadFailed()) {
        this.error.set(false);
        this.message.set(success);
      }
    } catch (e) {
      this.fail(e);
    } finally {
      this.busy.set(false);
    }
  }
  fail(e: unknown) {
    this.error.set(true);
    this.message.set(e instanceof Error ? e.message : 'Unable to complete this action.');
  }
}
