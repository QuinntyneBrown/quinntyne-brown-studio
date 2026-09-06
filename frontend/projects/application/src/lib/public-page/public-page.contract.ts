import { PublicItem } from './public-item';
import { PhotoView } from '@qbs/domain';
export interface IPublicPageService {
  kind: import('@angular/core').WritableSignal<string>;
  items: import('@angular/core').WritableSignal<PublicItem[]>;
  readonly galleryPhotos: import('@angular/core').Signal<PhotoView[]>;
  heading: import('@angular/core').WritableSignal<string>;
  body: import('@angular/core').WritableSignal<string>;
  message: import('@angular/core').WritableSignal<string>;
  loading: import('@angular/core').WritableSignal<boolean>;
  opened: import('@angular/core').WritableSignal<PhotoView | null>;
  initialize(): void;
  load(): Promise<void>;
}
