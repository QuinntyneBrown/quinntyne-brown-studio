import { AnalysisBatch } from '@qbs/domain/models';
export interface IAnalysisService {
  start(
    sessionId: string,
    photoIds: string[],
  ): Promise<{
    id: string;
  }>;
  get(id: string): Promise<AnalysisBatch>;
  retry(
    id: string,
    failedPhotoIds: string[],
  ): Promise<{
    id: string;
  }>;
}
