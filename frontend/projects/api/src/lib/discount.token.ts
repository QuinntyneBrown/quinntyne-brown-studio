import { InjectionToken } from '@angular/core';
import { IDiscountService } from './discount.contract';
export const DISCOUNT_SERVICE = new InjectionToken<IDiscountService>('DISCOUNT_SERVICE');
