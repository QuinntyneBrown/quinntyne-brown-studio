import { InjectionToken } from '@angular/core';
import { IPhotoService } from './photo.contract';
export const PHOTO_SERVICE = new InjectionToken<IPhotoService>('PHOTO_SERVICE');
