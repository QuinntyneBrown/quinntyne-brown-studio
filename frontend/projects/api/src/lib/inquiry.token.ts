import { InjectionToken } from '@angular/core';
import { IInquiryService } from './inquiry.contract';
export const INQUIRY_SERVICE = new InjectionToken<IInquiryService>('INQUIRY_SERVICE');
