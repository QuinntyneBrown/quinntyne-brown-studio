import { InjectionToken } from '@angular/core';
import { IDiscountsApi } from './discounts.contract';
export const DISCOUNTS_API = new InjectionToken<IDiscountsApi>('DiscountsApi');
