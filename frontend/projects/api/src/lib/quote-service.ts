import { inject } from '@angular/core';
import { createQuoteHttpTransport } from 'reckoner/http';
import type { IQuoteService } from './quote.contract';
import { RECKONER_PUBLIC_OPTIONS } from './reckoner-public-options.token';
/** Public Reckoner requests bypass Studio cookies and antiforgery. */
export function QuoteService(): IQuoteService {
  return createQuoteHttpTransport({
    fetch: globalThis.fetch.bind(globalThis),
    ...inject(RECKONER_PUBLIC_OPTIONS),
  });
}
