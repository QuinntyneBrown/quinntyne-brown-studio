import { InjectionToken } from '@angular/core';
import { IPhotographerService } from './photographer.contract';
export const PHOTOGRAPHER_SERVICE = new InjectionToken<IPhotographerService>('PHOTOGRAPHER_SERVICE');
