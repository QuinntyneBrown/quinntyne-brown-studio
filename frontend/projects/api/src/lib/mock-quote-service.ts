import { Injectable } from '@angular/core';
import { QuoteInput } from '@qbs/domain';
import { IQuoteService } from './quote.contract';

/** Bound only by the acceptance build. The page object controls each response. */
@Injectable()
export class MockQuoteService implements IQuoteService {
  private get fixture(): IQuoteService {
    const fixture = (globalThis as typeof globalThis & { __qbsQuoteMock?: IQuoteService })
      .__qbsQuoteMock;
    if (!fixture) throw new Error('The controlled quote fixture has not been installed.');
    return fixture;
  }
  getStudios() {
    return this.fixture.getStudios();
  }
  resolveLocation(address: string) {
    return this.fixture.resolveLocation(address);
  }
  calculate(input: QuoteInput) {
    return this.fixture.calculate(structuredClone(input));
  }
}
