import { InjectionToken } from '@angular/core';
import { IStudioClient } from './studio-client.contract';
export const STUDIO_CLIENT = new InjectionToken<IStudioClient>('STUDIO_CLIENT');
