export interface AvailabilityResult {
  startsAt: string;
  endsAt: string;
  available: boolean;
  photographerIds: string[];
  reasonCode: string | null;
}
