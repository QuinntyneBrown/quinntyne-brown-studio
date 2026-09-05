import { InjectionToken } from '@angular/core';
import { IAlbumsApi } from './albums.contract';
export const ALBUMS_API = new InjectionToken<IAlbumsApi>('AlbumsApi');
