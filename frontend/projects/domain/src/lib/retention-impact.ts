export interface RetentionImpact {
  id: string;
  months: number;
  expiresAt: string | null;
  version: number;
  state: string;
  impactRevision: string;
  photoCount: number;
  publishedReferences: number;
  unreviewedRequests: number;
}
