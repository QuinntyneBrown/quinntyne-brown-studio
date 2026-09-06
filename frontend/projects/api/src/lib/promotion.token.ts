import { InjectionToken } from '@angular/core';
import { IPromotionService } from './promotion.contract';
export const PROMOTION_SERVICE = new InjectionToken<IPromotionService>('PROMOTION_SERVICE');
