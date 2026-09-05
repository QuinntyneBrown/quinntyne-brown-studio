import { InjectionToken } from '@angular/core';
import { IAuthApi } from './auth.contract';
export const AUTH_API = new InjectionToken<IAuthApi>('AuthApi');
