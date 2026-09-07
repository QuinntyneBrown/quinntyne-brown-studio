import { InjectionToken } from '@angular/core';
import type { ReckonerPublicOptions } from './reckoner-public-options';
export const RECKONER_PUBLIC_OPTIONS = new InjectionToken<ReckonerPublicOptions>(
  'RECKONER_PUBLIC_OPTIONS',
  { factory: () => ({ apiBaseUrl: '', publishableKey: '' }) },
);
