import {
  PrintPreviewInput,
  PrintPreview,
  PrintRequestInput,
  PrintRequest,
} from '@qbs/domain/models';
export interface IPrintRequestService {
  preview(value: PrintPreviewInput): Promise<PrintPreview>;
  list(state?: string): Promise<PrintRequest[]>;
  get(id: string): Promise<PrintRequest>;
  submitted(id: string): Promise<PrintRequest>;
  submit(value: PrintRequestInput): Promise<PrintRequest>;
  review(id: string, expectedVersion: number): Promise<PrintRequest>;
}
