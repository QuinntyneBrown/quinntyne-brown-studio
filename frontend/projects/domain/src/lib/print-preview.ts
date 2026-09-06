import { PricedPrintLine } from './priced-print-line';
export interface PrintPreview {
  inputRevision: number;
  lines: PricedPrintLine[];
  total: string;
}
