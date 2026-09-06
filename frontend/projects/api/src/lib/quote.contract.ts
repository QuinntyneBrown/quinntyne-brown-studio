import { QuoteInput, QuoteResult, QuoteStudioOption, ResolvedLocation } from '@qbs/domain/models';
export interface IQuoteService {
  getStudios(): Promise<QuoteStudioOption[]>;
  resolveLocation(address: string): Promise<ResolvedLocation[]>;
  calculate(input: QuoteInput): Promise<QuoteResult>;
}
