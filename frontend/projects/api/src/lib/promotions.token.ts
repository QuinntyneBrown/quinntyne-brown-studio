import { InjectionToken } from '@angular/core';
import { IPromotionsApi } from './promotions.contract';
export const PROMOTIONS_API = new InjectionToken<IPromotionsApi>('PromotionsApi');
