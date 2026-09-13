import { InjectionToken } from '@angular/core';
import { ILaunchService } from './launch.contract';
export const LAUNCH_SERVICE = new InjectionToken<ILaunchService>('LAUNCH_SERVICE');
