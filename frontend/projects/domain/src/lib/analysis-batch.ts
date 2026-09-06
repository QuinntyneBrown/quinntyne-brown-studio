import { AnalysisPhoto } from './analysis-photo';
export interface AnalysisBatch {
  id: string;
  sessionId: string;
  photos: AnalysisPhoto[];
}
