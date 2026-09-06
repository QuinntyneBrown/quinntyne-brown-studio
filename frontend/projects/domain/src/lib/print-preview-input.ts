import { PrintSelection } from './print-selection';
export interface PrintPreviewInput {
  inputRevision: number;
  lines: Omit<PrintSelection, 'optionRevision'>[];
}
