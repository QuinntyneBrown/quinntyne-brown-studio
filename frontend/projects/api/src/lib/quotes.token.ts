import { InjectionToken } from '@angular/core';
import { IQuotesApi } from './quotes.contract';
export const QUOTES_API = new InjectionToken<IQuotesApi>('QuotesApi');
