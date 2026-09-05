import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { STUDIO_API } from '@qbs/api';
import { PhotoView, ApiError } from '@qbs/domain';
import { PhotoGrid, Notice, EmptyState, Dialog } from '@qbs/components';
@Component({
  selector: 'qbs-client-page',
  imports: [FormsModule, RouterLink, PhotoGrid, Notice, EmptyState, Dialog],
  templateUrl: './client-page.html',
  styleUrl: './client-page.css',
})
export class ClientPage implements OnInit {
  private api = inject(STUDIO_API);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  kind = signal('galleries');
  heading = signal('Your sessions');
  items = signal<any[]>([]);
  photos = signal<PhotoView[]>([]);
  selected = signal<string[]>([]);
  opened = signal<PhotoView | null>(null);
  message = signal('');
  error = signal(false);
  busy = signal(false);
  editing = signal(false);
  album: any = { name: '', version: 0 };
  printOptions = signal<any[]>([]);
  printLines: any[] = [];
  notes = '';
  reviewing = signal(false);
  confirmation = signal<string | null>(null);
  private submissionKey = crypto.randomUUID();
  ngOnInit() {
    this.route.paramMap.subscribe(() => void this.load());
  }
  async load() {
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
        this.items.set(await this.api.get<any[]>('client/' + this.kind()));
      else if (this.kind() === 'gallery') {
        const gallery = await this.api.get<any>('client/galleries/' + id);
        this.heading.set(gallery.name);
        this.photos.set(gallery.photos);
      } else if (this.kind() === 'album') {
        this.album = await this.api.get('client/albums/' + id);
        this.heading.set(this.album.name);
        this.photos.set(this.album.photos);
        this.selected.set(this.album.photos.map((p: PhotoView) => p.id));
      } else {
        await this.allPhotos();
        this.printOptions.set(await this.api.get('public/print-options'));
      }
    } catch (e) {
      this.fail(e);
    }
  }
  async allPhotos() {
    const galleries = await this.api.get<any[]>('client/galleries');
    const pages = await Promise.all(
      galleries.map((g) => this.api.get<any>('client/galleries/' + g.id)),
    );
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
      this.album = { name: '', version: 0 };
      this.selected.set([]);
    }
    const existing = this.photos();
    await this.allPhotos();
    this.photos.update((items) => [
      ...items,
      ...existing.filter((x) => !items.some((y) => x.id === y.id)),
    ]);
    this.editing.set(true);
  }
  async saveAlbum() {
    await this.action(async () => {
      const result = await this.api.send<any>(
        this.album.id ? 'PUT' : 'POST',
        'client/albums' + (this.album.id ? '/' + this.album.id : ''),
        {
          name: this.album.name,
          photoIds: this.selected(),
          orderedPhotoIds: this.selected(),
          expectedVersion: this.album.version,
        },
      );
      this.editing.set(false);
      await this.router.navigate(['/albums', result.id]);
      await this.load();
    }, 'Album saved.');
  }
  preparePrints() {
    this.printLines = this.selected().map((id) => {
      const prior = this.printLines.find((l) => l.photoId === id);
      return prior ?? { photoId: id, optionId: this.printOptions()[0]?.id ?? '', quantity: 1 };
    });
    this.reviewing.set(true);
  }
  photoName(id: string) {
    return this.photos().find((p) => p.id === id)?.name ?? 'Unavailable photo';
  }
  printTotal() {
    let cents = 0n;
    for (const line of this.printLines) {
      const price = String(this.option(line.optionId)?.unitPrice ?? '0').split('.');
      const unit = BigInt(price[0]) * 100n + BigInt((price[1] ?? '').padEnd(2, '0').slice(0, 2));
      if (!Number.isSafeInteger(line.quantity) || line.quantity < 1) return null;
      cents += unit * BigInt(line.quantity);
    }
    return (cents / 100n).toString() + '.' + (cents % 100n).toString().padStart(2, '0');
  }
  option(id: string) {
    return this.printOptions().find((p) => p.id === id);
  }
  async submitPrints() {
    this.busy.set(true);
    try {
      const result = await this.api.send<any>('POST', 'client/print-requests', {
        idempotencyKey: this.submissionKey,
        lines: this.printLines.map((l) => ({
          ...l,
          optionRevision: this.option(l.optionId)?.revision,
        })),
        notes: this.notes,
      });
      this.confirmation.set(result.id);
      this.message.set('Your print request has been received.');
      this.error.set(false);
    } catch (e) {
      if (e instanceof ApiError && e.status === 409) {
        this.printOptions.set(await this.api.get('public/print-options'));
        this.reviewing.set(false);
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
