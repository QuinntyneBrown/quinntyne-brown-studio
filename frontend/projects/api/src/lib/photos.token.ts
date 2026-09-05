import { InjectionToken } from '@angular/core';
import { IPhotosApi } from './photos.contract';
export const PHOTOS_API = new InjectionToken<IPhotosApi>('PhotosApi');
