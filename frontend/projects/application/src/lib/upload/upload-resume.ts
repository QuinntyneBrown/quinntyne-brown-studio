import { UploadBatch, UploadManifestEntry } from '@qbs/domain';
export interface UploadResume extends UploadBatch {
  manifest: UploadManifestEntry[];
  blocks: Record<string, string[]>;
}
