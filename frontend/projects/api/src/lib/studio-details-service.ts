import { Injectable, inject } from '@angular/core';
import { StudioDetails } from '@qbs/domain/models';
import { IStudioDetailsService } from './studio-details.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class StudioDetailsService implements IStudioDetailsService {
  private readonly transport = inject(STUDIO_CLIENT);
  get(): Promise<StudioDetails> {
    return this.transport.get<StudioDetails>('admin/studio-details');
  }
  save(value: StudioDetails): Promise<StudioDetails> {
    return this.transport.send<StudioDetails>('PUT', 'admin/studio-details', {
      ...value,
      expectedVersion: value.version,
    });
  }
}
