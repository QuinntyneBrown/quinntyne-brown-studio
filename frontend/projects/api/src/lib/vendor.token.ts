import { InjectionToken } from '@angular/core';
import { IVendorService } from './vendor.contract';
export const VENDOR_SERVICE = new InjectionToken<IVendorService>('VENDOR_SERVICE');
