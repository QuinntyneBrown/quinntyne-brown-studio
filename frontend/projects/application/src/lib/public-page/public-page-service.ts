import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PublicItem } from './public-item';
import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  CONTENT_SERVICE,
  PUBLIC_GALLERY_SERVICE,
  PRINT_OPTION_SERVICE,
  PROMOTION_SERVICE,
} from '@qbs/api';
import { PhotoView, ApiError } from '@qbs/domain';
import { IPublicPageService } from './public-page.contract';
@Injectable()
export class PublicPageService implements IPublicPageService {
  private content = inject(CONTENT_SERVICE);
  private galleries = inject(PUBLIC_GALLERY_SERVICE);
  private options = inject(PRINT_OPTION_SERVICE);
  private promotions = inject(PROMOTION_SERVICE);
  private readonly destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  kind = signal('home');
  items = signal<PublicItem[]>([]);
  readonly galleryPhotos = signal<PhotoView[]>([]);
  heading = signal('Photography with feeling.');
  body = signal('Honest moments. Thoughtfully captured.');
  message = signal('');
  loading = signal(true);
  opened = signal<PhotoView | null>(null);
  initialize() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => void this.load());
  }
  async load() {
    const kind = this.route.snapshot.data['kind'] ?? 'home';
    this.kind.set(kind);
    this.loading.set(true);
    this.message.set('');
    try {
      if (kind === 'home' || kind === 'services' || kind === 'contact') {
        try {
          const content = await this.content.published(kind);
          this.heading.set(content.heading);
          this.body.set(content.body);
        } catch (error) {
          if (!(error instanceof ApiError) || error.kind !== 'not-found') throw error;
          this.heading.set(
            kind === 'home'
              ? 'Photography with feeling.'
              : kind === 'services'
                ? 'Photography for your life.'
                : "Let's make something meaningful.",
          );
          this.body.set(
            'Weddings, events, headshots, and family portraits. Thoughtfully photographed.',
          );
        }
        this.items.set(await this.galleries.published());
      } else if (kind === 'gallery') {
        const gallery = await this.galleries.publishedGallery(
          this.route.snapshot.paramMap.get('slug')!,
        );
        this.heading.set(gallery.title);
        this.items.set(gallery.photos);
        this.galleryPhotos.set(gallery.photos);
      } else {
        this.items.set(
          await (kind === 'portfolio'
            ? this.galleries.published()
            : kind === 'prints'
              ? this.options.published()
              : this.promotions.published()),
        );
        this.heading.set(
          kind === 'portfolio'
            ? 'Selected work'
            : kind === 'prints'
              ? 'Made to be held.'
              : 'Packages & promotions',
        );
      }
    } catch (e) {
      this.message.set(e instanceof Error ? e.message : 'Unable to load.');
    } finally {
      this.loading.set(false);
    }
  }
}
