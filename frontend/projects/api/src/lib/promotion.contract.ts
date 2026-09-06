import { Editable, Promotion } from '@qbs/domain/models';
export interface IPromotionService {
  list(): Promise<Promotion[]>;
  get(id: string): Promise<Promotion>;
  save(value: Editable<Promotion>): Promise<Promotion>;
  published(): Promise<Promotion[]>;
}
