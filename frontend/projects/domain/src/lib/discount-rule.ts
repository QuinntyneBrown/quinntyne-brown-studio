export interface DiscountRule {
  id?: string;
  enabled: boolean;
  percentage: string | number;
  threshold?: number;
  weekdays: string[];
  code?: string | null;
  validFrom?: string | null;
  validTo?: string | null;
}
