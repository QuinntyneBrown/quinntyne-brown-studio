import { Injectable } from '@angular/core';
import type { QuoteRequest } from 'reckoner/behavior';
import type { IQuoteService } from './quote.contract';
/** Bound only by the acceptance build. The page object controls each cancellable response. */
@Injectable()
export class MockQuoteService implements IQuoteService {
  private get fixture(): IQuoteService | undefined {
    return (globalThis as typeof globalThis & { __qbsQuoteMock?: IQuoteService }).__qbsQuoteMock;
  }
  get configured() {
    return this.fixture?.configured ?? false;
  }
  loadDefinition() {
    return this.required().loadDefinition();
  }
  calculate(input: QuoteRequest) {
    return this.required().calculate(structuredClone(input));
  }
  resolveAddress(query: string) {
    return this.required().resolveAddress(query);
  }
  loadAvailability(month: string) {
    return this.required().loadAvailability(month);
  }
  private required() {
    const fixture = this.fixture;
    if (!fixture) throw new Error('The controlled quote fixture has not been installed.');
    return fixture;
  }
}
