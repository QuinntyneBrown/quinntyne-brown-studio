import { InjectionToken } from '@angular/core';
import { IInquiryInboxService } from './inquiry-inbox.contract';
export const INQUIRY_INBOX_SERVICE = new InjectionToken<IInquiryInboxService>(
  'INQUIRY_INBOX_SERVICE',
);
