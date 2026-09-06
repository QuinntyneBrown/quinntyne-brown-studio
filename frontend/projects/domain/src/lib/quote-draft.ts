import { QuoteInput } from './quote-input';
export interface QuoteDraft {
  service: string;
  date: string;
  endDate: string;
  start: string;
  end: string;
  startOffset: string;
  endOffset: string;
  assistantCount: number | null;
  equipmentUnits: number | null;
  lunchCount: number | null;
  code: string;
  locations: QuoteInput['locations'];
}
