import { PhotoView } from './photo-view';
export interface ClientGallery {
  id: string;
  name: string;
  startsAt: string;
  expiresAt: string | null;
  photos: PhotoView[];
}
