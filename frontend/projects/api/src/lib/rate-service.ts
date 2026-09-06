import { Injectable, inject } from '@angular/core';
import { RateConfiguration } from '@qbs/domain/models';
import { IRateService } from './rate.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class RateService implements IRateService {
  private readonly transport = inject(STUDIO_CLIENT);
  get(): Promise<RateConfiguration> {
    return this.transport.get<RateConfiguration>('admin/rates');
  }
  save(value: RateConfiguration): Promise<RateConfiguration> {
    return this.transport.send<RateConfiguration>('PUT', 'admin/rates', {
      ...value,
      expectedVersion: value.version,
    });
  }
}
