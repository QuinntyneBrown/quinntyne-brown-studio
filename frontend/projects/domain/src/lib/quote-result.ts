import { Money } from './money';
export interface QuoteResult {
  inputRevision: number;
  configurationRevision: number;
  lines: {
    kind: string;
    quantity: string;
    amount: Money;
    locationIndex: number | null;
  }[];
  subtotal: Money;
  discount: {
    percentage: string;
    amount: Money;
    kind: string | null;
    codeError: string | null;
  };
  total: Money;
  availability: {
    available: boolean;
    photographerIds: string[];
    reasonCode: string | null;
  };
}
