import { Injectable, inject } from '@angular/core';
import { DiscountConfiguration } from '@qbs/domain/models';
import { IDiscountService } from './discount.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class DiscountService implements IDiscountService {
  private readonly transport = inject(STUDIO_CLIENT);
  get(): Promise<DiscountConfiguration> {
    return this.transport.get<DiscountConfiguration>('admin/discounts');
  }
  save(value: DiscountConfiguration): Promise<DiscountConfiguration> {
    return this.transport.send<DiscountConfiguration>('PUT', 'admin/discounts', {
      ...value,
      expectedVersion: value.version,
    });
  }
}
