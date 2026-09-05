import { InjectionToken } from '@angular/core';
import { IVendorsApi } from './vendors.contract';
export const VENDORS_API = new InjectionToken<IVendorsApi>('VendorsApi');
