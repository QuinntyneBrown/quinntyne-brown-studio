import { InjectionToken } from '@angular/core';
import { IAlbumService } from './album.contract';
export const ALBUM_SERVICE = new InjectionToken<IAlbumService>('ALBUM_SERVICE');
