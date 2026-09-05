import { InjectionToken } from '@angular/core';
import { IStudioApi } from './studio-api.contract';
export const STUDIO_API = new InjectionToken<IStudioApi>('STUDIO_API');
