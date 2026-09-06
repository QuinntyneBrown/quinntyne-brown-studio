import { InjectionToken } from '@angular/core';
import { IAvailabilityService } from './availability.contract';
export const AVAILABILITY_SERVICE = new InjectionToken<IAvailabilityService>('AVAILABILITY_SERVICE');
