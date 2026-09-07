import { InjectionToken } from '@angular/core';
import { IReckonerAdminService } from './reckoner-admin.contract';
export const RECKONER_ADMIN_SERVICE = new InjectionToken<IReckonerAdminService>(
  'Reckoner admin access',
);
