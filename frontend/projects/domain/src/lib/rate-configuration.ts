export interface RateConfiguration {
  id: string;
  version: number;
  serviceRates: Record<string, string | number | null>;
  costRates: Record<string, string | number | null>;
}
