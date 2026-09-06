import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Injectable, inject, signal, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  CLIENT_GALLERY_SERVICE,
  ALBUM_SERVICE,
  PRINT_OPTION_SERVICE,
  PRINT_REQUEST_SERVICE,
} from '@qbs/api';
import { PhotoView, ApiError, Album, PrintOption, PrintSelection, PrintPreview } from '@qbs/domain';
import { IClientPageService } from '@qbs/api';
@Injectable()
export class ClientPageService implements IClientPageService, OnDestroy {
  readonly loading = signal(true);
  readonly loadFailed = signal(false);
  private galleries = inject(CLIENT_GALLERY_SERVICE);
  private albums = inject(ALBUM_SERVICE);
  private options = inject(PRINT_OPTION_SERVICE);
  private prints = inject(PRINT_REQUEST_SERVICE);
  private readonly destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  kind = signal('galleries');
  heading = signal('Your sessions');
  items = signal<
    {
      id: string;
      name: string;
    }[]
  >([]);
  photos = signal<PhotoView[]>([]);
  selected = signal<string[]>([]);
  opened = signal<PhotoView | null>(null);
  message = signal('');
  error = signal(false);
  busy = signal(false);
  editing = signal(false);
  album = signal<
    Partial<Album> & {
      name: string;
    }
  >({ name: '', version: 0 });
  printOptions = signal<PrintOption[]>([]);
  printLines = signal<Omit<PrintSelection, 'optionRevision'>[]>([]);
  notes = signal('');
  reviewing = signal(false);
  confirmation = signal<string | null>(null);
  readonly preview = signal<PrintPreview | null>(null);
  readonly pricing = signal<'idle' | 'updating' | 'current' | 'failed' | 'invalid'>('idle');
  private priceRevision = 0;
  private priceTimer?: ReturnType<typeof setTimeout>;
  private destroyed = false;
  ngOnDestroy() {
    this.destroyed = true;
    this.priceRevision++;
    clearTimeout(this.priceTimer);
  }
  private submissionKey = crypto.randomUUID();
  initialize() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => void this.load());
  }
  async load() {
    this.loadFailed.set(false);
    this.loading.set(true);
    this.error.set(false);
    this.message.set('');
    this.kind.set(this.route.snapshot.data['kind']);
    this.heading.set(
      (
        {
          galleries: 'Your sessions',
          gallery: 'Your photographs',
          albums: 'Your albums',
          album: 'Your album',
          prints: 'Made to be held.',
        } as Record<string, string>
      )[this.kind()],
    );
    this.message.set('');
    this.photos.set([]);
    this.selected.set([]);
    this.editing.set(false);
    try {
      const id = this.route.snapshot.paramMap.get('id');
      if (this.kind() === 'galleries' || this.kind() === 'albums')
        this.items.set(
          await (this.kind() === 'galleries' ? this.galleries.list() : this.albums.list()),
        );
      else if (this.kind() === 'gallery') {
        const gallery = await this.galleries.get(id!);
        this.heading.set(gallery.name);
        this.photos.set(gallery.photos);
      } else if (this.kind() === 'album') {
        this.album.set(await this.albums.get(id!));
        this.heading.set(this.album().name);
        this.photos.set(this.album().photos ?? []);
        this.selected.set((this.album().photos ?? []).map((p: PhotoView) => p.id));
      } else {
        await this.allPhotos();
        this.printOptions.set(await this.options.published());
      }
    } catch (e) {
      this.loadFailed.set(true);
      this.fail(e);
    } finally {
      this.loading.set(false);
    }
  }
  async allPhotos() {
    const galleries = await this.galleries.list();
    const pages = await Promise.all(galleries.map((g) => this.galleries.get(g.id)));
    this.photos.set(pages.flatMap((g) => g.photos));
  }
  toggle(id: string) {
    this.selected.update((ids) => (ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id]));
    this.reviewing.set(false);
  }
  move(id: string, delta: number) {
    this.selected.update((ids) => {
      const copy = [...ids];
      const i = copy.indexOf(id),
        target = i + delta;
      if (target >= 0 && target < copy.length) [copy[i], copy[target]] = [copy[target], copy[i]];
      return copy;
    });
  }
  async editAlbum(isNew = false) {
    if (isNew) {
      this.album.set({ name: '', version: 0 });
      this.selected.set([]);
    }
    const existing = this.photos();
    try {
      await this.allPhotos();
    } catch (error) {
      this.fail(error);
      return;
    }
    this.photos.update((items) => [
      ...items,
      ...existing.filter((x) => !items.some((y) => x.id === y.id)),
    ]);
    this.editing.set(true);
  }
  async saveAlbum() {
    await this.action(async () => {
      const result = await this.albums.save({
        id: this.album().id,
        version: this.album().version,
        name: this.album().name,
        orderedPhotoIds: this.selected(),
      });
      this.editing.set(false);
      await this.router.navigate(['/albums', result.id]);
      await this.load();
    }, 'Album saved.');
  }
  preparePrints() {
    this.printLines.set(
      this.selected().map((id) => {
        const prior = this.printLines().find((l) => l.photoId === id);
        return prior ?? { photoId: id, optionId: this.printOptions()[0]?.id ?? '', quantity: 1 };
      }),
    );
    this.reviewing.set(true);
    this.priceChanged();
  }
  photoName(id: string) {
    return this.photos().find((p) => p.id === id)?.name ?? 'Unavailable photo';
  }
  printTotal() {
    return this.preview()?.total ?? null;
  }
  notesChanged() {
    this.submissionKey = crypto.randomUUID();
  }
  priceChanged() {
    this.priceRevision++;
    this.preview.set(null);
    this.submissionKey = crypto.randomUUID();
    clearTimeout(this.priceTimer);
    if (
      !this.printLines().length ||
      this.printLines().some(
        (line) => !line.optionId || !Number.isSafeInteger(line.quantity) || line.quantity < 1,
      )
    ) {
      this.pricing.set('invalid');
      return;
    }
    this.pricing.set('updating');
    this.priceTimer = setTimeout(() => void this.refreshPrice(), 150);
  }
  async refreshPrice() {
    const revision = ++this.priceRevision;
    this.preview.set(null);
    this.pricing.set('updating');
    this.message.set('');
    try {
      const preview = await this.prints.preview({
        inputRevision: revision,
        lines: structuredClone(this.printLines()),
      });
      if (this.destroyed || revision !== this.priceRevision) return;
      if (preview.inputRevision !== revision)
        throw new Error('Price review could not be matched to these selections. Try again.');
      this.preview.set(preview);
      this.pricing.set('current');
      this.error.set(false);
    } catch (error) {
      if (this.destroyed || revision !== this.priceRevision) return;
      this.pricing.set('failed');
      this.fail(error);
    }
  }
  async submitPrints() {
    const preview = this.preview();
    if (this.busy() || !preview || this.pricing() !== 'current') return;
    this.busy.set(true);
    try {
      const result = await this.prints.submit({
        idempotencyKey: this.submissionKey,
        lines: preview.lines.map((line) => ({
          photoId: line.photoId,
          optionId: line.optionId,
          optionRevision: line.optionRevision,
          quantity: line.quantity,
        })),
        notes: this.notes(),
      });
      this.confirmation.set(result.id);
      this.message.set('Your print request has been received.');
      this.error.set(false);
    } catch (e) {
      if (e instanceof ApiError && e.kind === 'conflict') {
        this.preview.set(null);
        this.pricing.set('failed');
        this.submissionKey = crypto.randomUUID();
      }
      this.fail(e);
    } finally {
      this.busy.set(false);
    }
  }
  async action(fn: () => Promise<unknown>, message: string) {
    this.busy.set(true);
    try {
      await fn();
      this.error.set(false);
      this.message.set(message);
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
