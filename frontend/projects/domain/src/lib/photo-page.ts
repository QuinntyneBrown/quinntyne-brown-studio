import { PhotoView } from './photo-view';
export interface PhotoPage {
  photos: PhotoView[];
  nextCursor: string | null;
}
