import { DiscountRule } from './discount-rule';
import { TimeWindow } from './time-window';
import { ResolvedLocation } from './resolved-location';
export interface SettingsDraft {
  id?: string;
  version: number;
  name: string;
  email: string;
  hourlyFee: string | number;
  enabled: boolean;
  isBase: boolean;
  resolvedAddress: ResolvedLocation | null;
  pageKey: string;
  heading: string;
  body: string;
  publish: boolean;
  publishedHeading: string | null;
  publishedBody: string | null;
  serviceRates: Record<string, string | number | null>;
  costRates: Record<string, string | number | null>;
  advanceRule: DiscountRule;
  weekdayRule: DiscountRule;
  codeRules: DiscountRule[];
  photographerId: string;
  workingWindows: TimeWindow[];
  unavailableWindows: TimeWindow[];
  buffers: {
    before: number;
    after: number;
  };
}
