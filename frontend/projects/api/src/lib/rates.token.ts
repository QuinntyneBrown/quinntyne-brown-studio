import { InjectionToken } from '@angular/core';
import { IRatesApi } from './rates.contract';
export const RATES_API = new InjectionToken<IRatesApi>('RatesApi');
