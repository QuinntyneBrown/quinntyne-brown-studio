import { InjectionToken } from '@angular/core';
import { IPrintRequestsApi } from './print-requests.contract';
export const PRINT_REQUESTS_API = new InjectionToken<IPrintRequestsApi>('PrintRequestsApi');
