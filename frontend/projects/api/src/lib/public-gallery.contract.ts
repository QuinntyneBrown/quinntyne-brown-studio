import { Editable, PublicGallery } from '@qbs/domain/models';
export interface IPublicGalleryService {
  list(): Promise<PublicGallery[]>;
  get(id: string): Promise<PublicGallery>;
  save(value: Editable<PublicGallery>): Promise<PublicGallery>;
  published(): Promise<PublicGallery[]>;
  publishedGallery(slug: string): Promise<PublicGallery>;
}
