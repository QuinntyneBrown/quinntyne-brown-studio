import { PrintSelection } from './print-selection';
export interface PrintRequestInput {
  idempotencyKey: string;
  lines: PrintSelection[];
  notes: string;
}
