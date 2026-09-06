import { ResolvedLocation } from './resolved-location';
export interface QuoteStudioOption {
  id: string;
  name: string;
  resolvedAddress: ResolvedLocation;
  hourlyFee: string;
}
