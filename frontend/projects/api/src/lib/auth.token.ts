import { InjectionToken } from '@angular/core';
import { IAuthService } from './auth.contract';
export const AUTH_SERVICE = new InjectionToken<IAuthService>('AUTH_SERVICE');
