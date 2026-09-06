import { Provider } from '@angular/core';
import { QUOTE_SERVICE, QuoteService } from '@qbs/api';
export function quoteProvider(): Provider {
  return { provide: QUOTE_SERVICE, useClass: QuoteService };
}
