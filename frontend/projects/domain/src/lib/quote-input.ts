import { ResolvedLocation } from './resolved-location';
export interface QuoteInput {
  service: string;
  startsAt: string;
  endsAt: string;
  locations: {
    location: ResolvedLocation;
    parkingAmount: string;
    studioId: string | null;
    studioHours: string;
  }[];
  assistantCount: number;
  equipmentUnits: number;
  lunchCount: number;
  code: string;
  photographerId: string | null;
  inputRevision: number;
}
