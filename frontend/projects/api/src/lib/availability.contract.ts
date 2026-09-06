import { QuoteInput, AvailabilityResult } from '@qbs/domain/models';
export interface IAvailabilityService {
  check(input: QuoteInput): Promise<AvailabilityResult>;
}
