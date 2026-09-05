import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { STUDIO_API } from '@qbs/api';
import { PhotoView } from '@qbs/domain';
import { PhotoGrid, EmptyState, Notice, Dialog } from '@qbs/components';
@Component({
  selector: 'qbs-public-page',
  imports: [RouterLink, PhotoGrid, EmptyState, Notice, Dialog],
  templateUrl: './public-page.html',
  styleUrl: './public-page.css',
})
export class PublicPage implements OnInit {
  private api = inject(STUDIO_API);
  private route = inject(ActivatedRoute);
  kind = signal('home');
  items = signal<any[]>([]);
  heading = signal('Photography with feeling.');
  body = signal('Honest moments. Thoughtfully captured.');
  message = signal('');
  loading = signal(true);
  opened = signal<PhotoView | null>(null);
  ngOnInit() {
    this.route.paramMap.subscribe(() => void this.load());
  }
  async load() {
    const kind = this.route.snapshot.data['kind'] ?? 'home';
    this.kind.set(kind);
    this.loading.set(true);
    this.message.set('');
    try {
      if (kind === 'home' || kind === 'services' || kind === 'contact') {
        try {
          const content = await this.api.get<{ heading: string; body: string }>(
            'public/content/' + kind,
          );
          this.heading.set(content.heading);
          this.body.set(content.body);
        } catch {
          this.heading.set(
            kind === 'home'
              ? 'Photography with feeling.'
              : kind === 'services'
                ? 'Photography for your life.'
                : 'Let’s make something meaningful.',
          );
          this.body.set(
            'Weddings, events, headshots, and family portraits. Thoughtfully photographed.',
          );
        }
        this.items.set(await this.api.get<any[]>('public/galleries'));
      } else if (kind === 'gallery') {
        const gallery = await this.api.get<any>(
          'public/galleries/' + this.route.snapshot.paramMap.get('slug'),
        );
        this.heading.set(gallery.title);
        this.items.set(gallery.photos);
      } else {
        const endpoint =
          kind === 'portfolio' ? 'galleries' : kind === 'prints' ? 'print-options' : 'promotions';
        this.items.set(await this.api.get<any[]>('public/' + endpoint));
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
