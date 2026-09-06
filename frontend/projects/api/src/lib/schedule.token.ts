import { InjectionToken } from '@angular/core';
import { IScheduleService } from './schedule.contract';
export const SCHEDULE_SERVICE = new InjectionToken<IScheduleService>('SCHEDULE_SERVICE');
