import { Injectable, inject } from '@angular/core';
import {
  PrintPreviewInput,
  PrintPreview,
  PrintRequestInput,
  PrintRequest,
} from '@qbs/domain/models';
import { IPrintRequestService } from './print-request.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class PrintRequestService implements IPrintRequestService {
  private readonly transport = inject(STUDIO_CLIENT);
  preview(value: PrintPreviewInput): Promise<PrintPreview> {
    return this.transport.send<PrintPreview>('POST', 'client/print-requests/preview', value);
  }
  list(state?: string): Promise<PrintRequest[]> {
    return this.transport.get<PrintRequest[]>(
      'admin/print-requests' + (state ? '?state=' + encodeURIComponent(state) : ''),
    );
  }
  get(id: string): Promise<PrintRequest> {
    return this.transport.get<PrintRequest>('admin/print-requests/' + encodeURIComponent(id));
  }
  submitted(id: string): Promise<PrintRequest> {
    return this.transport.get<PrintRequest>('client/print-requests/' + encodeURIComponent(id));
  }
  submit(value: PrintRequestInput): Promise<PrintRequest> {
    return this.transport.send<PrintRequest>('POST', 'client/print-requests', value);
  }
  review(id: string, expectedVersion: number): Promise<PrintRequest> {
    return this.transport.send<PrintRequest>(
      'POST',
      `admin/print-requests/${encodeURIComponent(id)}/review`,
      { expectedVersion },
    );
  }
}
