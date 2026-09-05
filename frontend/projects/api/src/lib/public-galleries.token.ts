import { InjectionToken } from '@angular/core';
import { IPublicGalleriesApi } from './public-galleries.contract';
export const PUBLIC_GALLERIES_API = new InjectionToken<IPublicGalleriesApi>('PublicGalleriesApi');
