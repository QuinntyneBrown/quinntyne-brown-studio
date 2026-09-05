import { InjectionToken } from '@angular/core';
import { ISessionsApi } from './sessions.contract';
export const SESSIONS_API = new InjectionToken<ISessionsApi>('SessionsApi');
