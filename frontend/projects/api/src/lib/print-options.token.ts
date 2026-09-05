import { InjectionToken } from '@angular/core';
import { IPrintOptionsApi } from './print-options.contract';
export const PRINT_OPTIONS_API = new InjectionToken<IPrintOptionsApi>('PrintOptionsApi');
