import { InjectionToken } from '@angular/core';
import { IRetentionService } from './retention.contract';
export const RETENTION_SERVICE = new InjectionToken<IRetentionService>('RETENTION_SERVICE');
