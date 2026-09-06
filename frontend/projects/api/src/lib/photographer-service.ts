import { Injectable, inject } from '@angular/core';
import { Editable, Photographer } from '@qbs/domain/models';
import { IPhotographerService } from './photographer.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class PhotographerService implements IPhotographerService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<Photographer[]> {
    return this.transport.get<Photographer[]>('admin/photographers');
  }
  get(id: string): Promise<Photographer> {
    return this.transport.get<Photographer>(`admin/photographers/${encodeURIComponent(id)}`);
  }
  save(value: Editable<Photographer>): Promise<Photographer> {
    return this.transport.send<Photographer>(
      value.id ? 'PUT' : 'POST',
      'admin/photographers' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
}
