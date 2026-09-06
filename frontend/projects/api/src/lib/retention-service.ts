import { Injectable, inject } from '@angular/core';
import { StudioSession, RetentionImpact } from '@qbs/domain/models';
import { IRetentionService } from './retention.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class RetentionService implements IRetentionService {
  private readonly transport = inject(STUDIO_CLIENT);
  get(sessionId: string): Promise<RetentionImpact> {
    return this.transport.get<RetentionImpact>(
      `admin/sessions/${encodeURIComponent(sessionId)}/retention`,
    );
  }
  extend(
    sessionId: string,
    months: number,
    expiresAt: string | null,
    expectedVersion: number,
  ): Promise<StudioSession> {
    return this.transport.send<StudioSession>(
      'PUT',
      `admin/sessions/${encodeURIComponent(sessionId)}/retention`,
      { months, expiresAt, expectedVersion },
    );
  }
  deletePhotos(
    sessionId: string,
    impactRevision: string,
  ): Promise<{
    jobId: string;
  }> {
    return this.transport.send<{
      jobId: string;
    }>('POST', `admin/sessions/${encodeURIComponent(sessionId)}/photo-deletion`, {
      impactRevision,
      confirm: true,
    });
  }
}
