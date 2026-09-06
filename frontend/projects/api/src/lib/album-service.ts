import { Injectable, inject } from '@angular/core';
import { Album, AlbumInput } from '@qbs/domain/models';
import { IAlbumService } from './album.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class AlbumService implements IAlbumService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<Album[]> {
    return this.transport.get<Album[]>('client/albums');
  }
  get(id: string): Promise<Album> {
    return this.transport.get<Album>('client/albums/' + encodeURIComponent(id));
  }
  save(value: AlbumInput): Promise<Album> {
    return this.transport.send<Album>(
      value.id ? 'PUT' : 'POST',
      'client/albums' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
}
