import { Provider } from '@angular/core';
import { QUOTE_SERVICE, MockQuoteService } from '@qbs/api';
export function quoteProvider(): Provider {
  return { provide: QUOTE_SERVICE, useClass: MockQuoteService };
}
