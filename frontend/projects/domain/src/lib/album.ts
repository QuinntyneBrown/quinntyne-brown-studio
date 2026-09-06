import { PhotoView } from './photo-view';
export interface Album {
  id: string;
  version: number;
  name: string;
  photos: PhotoView[];
  photoIds?: string[];
  count?: number;
}
