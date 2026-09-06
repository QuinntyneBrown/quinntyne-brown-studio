import { InjectionToken } from '@angular/core';
import { IAccountService } from './account.contract';
export const ACCOUNT_SERVICE = new InjectionToken<IAccountService>('ACCOUNT_SERVICE');
