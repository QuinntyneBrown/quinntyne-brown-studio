import { Editable, Equipment } from '@qbs/domain/models';
export interface IEquipmentService {
  list(): Promise<Equipment[]>;
  get(id: string): Promise<Equipment>;
  save(value: Editable<Equipment>): Promise<Equipment>;
}
