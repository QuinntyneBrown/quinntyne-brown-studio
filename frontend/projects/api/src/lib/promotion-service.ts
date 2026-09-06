import { Injectable, inject } from '@angular/core';
import { Editable, Promotion } from '@qbs/domain/models';
import { IPromotionService } from './promotion.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class PromotionService implements IPromotionService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<Promotion[]> {
    return this.transport.get<Promotion[]>('admin/promotions');
  }
  get(id: string): Promise<Promotion> {
    return this.transport.get<Promotion>(`admin/promotions/${encodeURIComponent(id)}`);
  }
  save(value: Editable<Promotion>): Promise<Promotion> {
    return this.transport.send<Promotion>(
      value.id ? 'PUT' : 'POST',
      'admin/promotions' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
  published(): Promise<Promotion[]> {
    return this.transport.get<Promotion[]>('public/promotions');
  }
}
