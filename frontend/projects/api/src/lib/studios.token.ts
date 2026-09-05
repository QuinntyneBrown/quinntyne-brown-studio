import { InjectionToken } from '@angular/core';
import { IStudiosApi } from './studios.contract';
export const STUDIOS_API = new InjectionToken<IStudiosApi>('StudiosApi');
