import { Injectable, inject } from '@angular/core';
import { Inquiry } from '@qbs/domain/models';
import { IInquiryService } from './inquiry.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class InquiryService implements IInquiryService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(state?: string): Promise<Inquiry[]> {
    return this.transport.get<Inquiry[]>(
      'admin/inquiries' + (state ? '?state=' + encodeURIComponent(state) : ''),
    );
  }
  get(id: string): Promise<Inquiry> {
    return this.transport.get<Inquiry>('admin/inquiries/' + encodeURIComponent(id));
  }
  review(id: string, expectedVersion: number): Promise<Inquiry> {
    return this.transport.send<Inquiry>(
      'POST',
      `admin/inquiries/${encodeURIComponent(id)}/review`,
      { expectedVersion },
    );
  }
}
