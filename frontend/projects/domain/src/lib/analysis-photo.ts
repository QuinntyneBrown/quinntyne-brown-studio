import { AnalysisFinding } from './analysis-finding';
export interface AnalysisPhoto {
  id: string;
  photoId: string;
  state: string;
  error: string | null;
  result: {
    photoId: string;
    modelVersion: string;
    promptVersion: string;
    recommendation: string;
    findings: AnalysisFinding[];
  } | null;
}
