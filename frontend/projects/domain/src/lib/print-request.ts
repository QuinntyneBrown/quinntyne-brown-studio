import { PricedPrintLine } from './priced-print-line';
export interface PrintRequest {
  id: string;
  version: number;
  clientId: string;
  lines: PricedPrintLine[];
  notes: string;
  total: string;
  state: string;
  reviewedBy: string | null;
  reviewedAt: string | null;
}
