import { DiscountConfiguration } from '@qbs/domain/models';
export interface IDiscountService {
  get(): Promise<DiscountConfiguration>;
  save(value: DiscountConfiguration): Promise<DiscountConfiguration>;
}
