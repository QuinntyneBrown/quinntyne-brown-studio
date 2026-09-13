import { InjectionToken } from '@angular/core';
import { ILaunchGateService } from './launch-gate.contract';
export const LAUNCH_GATE_SERVICE = new InjectionToken<ILaunchGateService>('LAUNCH_GATE_SERVICE');
