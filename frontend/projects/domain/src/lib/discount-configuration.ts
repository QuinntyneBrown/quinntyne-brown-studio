import { DiscountRule } from './discount-rule';
export interface DiscountConfiguration {
  id: string;
  version: number;
  advanceRule: DiscountRule;
  weekdayRule: DiscountRule;
  codeRules: DiscountRule[];
}
