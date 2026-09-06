export interface StudioSession {
  id: string;
  version: number;
  name: string;
  service: string;
  startsAt: string;
  endsAt: string;
  photographerId: string | null;
  clientIds: string[];
  retentionMonths: number;
  expiresAt: string | null;
  retentionState: string;
}
