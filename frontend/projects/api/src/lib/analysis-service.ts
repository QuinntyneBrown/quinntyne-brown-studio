import { Injectable, inject } from '@angular/core';
import { AnalysisBatch } from '@qbs/domain/models';
import { IAnalysisService } from './analysis.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class AnalysisService implements IAnalysisService {
  private readonly transport = inject(STUDIO_CLIENT);
  start(
    sessionId: string,
    photoIds: string[],
  ): Promise<{
    id: string;
  }> {
    return this.transport.send<{
      id: string;
    }>('POST', `admin/sessions/${encodeURIComponent(sessionId)}/analysis`, { photoIds });
  }
  get(id: string): Promise<AnalysisBatch> {
    return this.transport.get<AnalysisBatch>('admin/analysis/' + encodeURIComponent(id));
  }
  retry(
    id: string,
    failedPhotoIds: string[],
  ): Promise<{
    id: string;
  }> {
    return this.transport.send<{
      id: string;
    }>('POST', `admin/analysis/${encodeURIComponent(id)}/retry`, { failedPhotoIds });
  }
}
