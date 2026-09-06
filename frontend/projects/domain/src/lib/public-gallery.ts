import { PhotoView } from './photo-view';
export interface PublicGallery {
  id: string;
  version: number;
  title: string;
  slug: string;
  photoIds: string[];
  published: boolean;
  photos: PhotoView[];
}
