import { InjectionToken } from '@angular/core';
import { ISessionService } from './session.contract';
export const SESSION_SERVICE = new InjectionToken<ISessionService>('SESSION_SERVICE');
