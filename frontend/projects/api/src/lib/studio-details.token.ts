import { InjectionToken } from '@angular/core';
import { IStudioDetailsService } from './studio-details.contract';
export const STUDIO_DETAILS_SERVICE = new InjectionToken<IStudioDetailsService>(
  'STUDIO_DETAILS_SERVICE',
);
