import { InjectionToken } from '@angular/core';
import { IQuoteService } from './quote.contract';
export const QUOTE_SERVICE = new InjectionToken<IQuoteService>('QUOTE_SERVICE');
