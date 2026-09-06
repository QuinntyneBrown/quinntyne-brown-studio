import { Injectable, inject } from '@angular/core';
import { Editable, PublicGallery } from '@qbs/domain/models';
import { IPublicGalleryService } from './public-gallery.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class PublicGalleryService implements IPublicGalleryService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<PublicGallery[]> {
    return this.transport.get<PublicGallery[]>('admin/public-galleries');
  }
  get(id: string): Promise<PublicGallery> {
    return this.transport.get<PublicGallery>(`admin/public-galleries/${encodeURIComponent(id)}`);
  }
  save(value: Editable<PublicGallery>): Promise<PublicGallery> {
    return this.transport.send<PublicGallery>(
      value.id ? 'PUT' : 'POST',
      'admin/public-galleries' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
  published(): Promise<PublicGallery[]> {
    return this.transport.get<PublicGallery[]>('public/galleries');
  }
  publishedGallery(slug: string): Promise<PublicGallery> {
    return this.transport.get<PublicGallery>('public/galleries/' + encodeURIComponent(slug));
  }
}
