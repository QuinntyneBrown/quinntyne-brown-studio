import { InjectionToken } from '@angular/core';
import { IRateService } from './rate.contract';
export const RATE_SERVICE = new InjectionToken<IRateService>('RATE_SERVICE');
