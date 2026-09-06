import { ResolvedLocation } from './resolved-location';
export interface StudioConfiguration {
  id: string;
  version: number;
  name: string;
  hourlyFee: string | number;
  enabled: boolean;
  isBase: boolean;
  resolvedAddress: ResolvedLocation | null;
}
