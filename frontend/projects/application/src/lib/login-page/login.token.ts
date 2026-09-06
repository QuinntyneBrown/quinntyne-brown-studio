import { InjectionToken } from '@angular/core';
import { ILoginService } from './login.contract';
export const LOGIN_SERVICE = new InjectionToken<ILoginService>('LOGIN_SERVICE');
