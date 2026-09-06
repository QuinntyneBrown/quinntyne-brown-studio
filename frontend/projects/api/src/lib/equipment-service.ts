import { Injectable, inject } from '@angular/core';
import { Editable, Equipment } from '@qbs/domain/models';
import { IEquipmentService } from './equipment.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class EquipmentService implements IEquipmentService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<Equipment[]> {
    return this.transport.get<Equipment[]>('admin/equipment');
  }
  get(id: string): Promise<Equipment> {
    return this.transport.get<Equipment>(`admin/equipment/${encodeURIComponent(id)}`);
  }
  save(value: Editable<Equipment>): Promise<Equipment> {
    return this.transport.send<Equipment>(
      value.id ? 'PUT' : 'POST',
      'admin/equipment' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
}
