import { Injectable, inject } from '@angular/core';
import { Editable, StudioConfiguration } from '@qbs/domain/models';
import { IStudioService } from './studio.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class StudioService implements IStudioService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<StudioConfiguration[]> {
    return this.transport.get<StudioConfiguration[]>('admin/studios');
  }
  get(id: string): Promise<StudioConfiguration> {
    return this.transport.get<StudioConfiguration>(`admin/studios/${encodeURIComponent(id)}`);
  }
  save(value: Editable<StudioConfiguration>): Promise<StudioConfiguration> {
    return this.transport.send<StudioConfiguration>(
      value.id ? 'PUT' : 'POST',
      'admin/studios' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
}
