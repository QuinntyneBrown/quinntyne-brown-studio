import { StudioSession, RetentionImpact } from '@qbs/domain/models';
export interface IRetentionService {
  get(sessionId: string): Promise<RetentionImpact>;
  extend(
    sessionId: string,
    months: number,
    expiresAt: string | null,
    expectedVersion: number,
  ): Promise<StudioSession>;
  deletePhotos(
    sessionId: string,
    impactRevision: string,
  ): Promise<{
    jobId: string;
  }>;
}
