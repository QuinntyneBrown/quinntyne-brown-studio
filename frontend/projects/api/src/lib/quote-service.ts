import { Injectable, inject } from '@angular/core';
import {
  ApiError,
  QuoteFailure,
  QuoteInput,
  QuoteResult,
  QuoteStudioOption,
  ResolvedLocation,
} from '@qbs/domain/models';
import { IQuoteService } from './quote.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class QuoteService implements IQuoteService {
  private readonly transport = inject(STUDIO_CLIENT);
  getStudios() {
    return this.transport.get<QuoteStudioOption[]>('public/studios');
  }
  resolveLocation(address: string) {
    return this.transport.send<ResolvedLocation[]>('POST', 'public/locations/resolve', { address });
  }
  async calculate(input: QuoteInput) {
    try {
      return await this.transport.send<QuoteResult>('POST', 'public/quotes/calculate', input);
    } catch (error) {
      if (error instanceof ApiError)
        throw new QuoteFailure(
          error.kind === 'invalid' ? 'invalid' : 'unavailable',
          error.message,
          error.errors,
        );
      throw error;
    }
  }
}
