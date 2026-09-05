import { InjectionToken } from '@angular/core';
import { IPhotographersApi } from './photographers.contract';
export const PHOTOGRAPHERS_API = new InjectionToken<IPhotographersApi>('PhotographersApi');
