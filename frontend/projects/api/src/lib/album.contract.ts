import { Album, AlbumInput } from '@qbs/domain/models';
export interface IAlbumService {
  list(): Promise<Album[]>;
  get(id: string): Promise<Album>;
  save(value: AlbumInput): Promise<Album>;
}
