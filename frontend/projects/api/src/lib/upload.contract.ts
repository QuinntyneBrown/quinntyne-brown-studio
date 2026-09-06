import { UploadManifestEntry, UploadBatch, UploadGrant, FinalizedPhoto } from '@qbs/domain/models';
export interface IUploadService {
  create(sessionId: string, files: UploadManifestEntry[]): Promise<UploadBatch>;
  status(id: string): Promise<UploadBatch>;
  renew(batchId: string, photoId: string): Promise<UploadGrant>;
  complete(batchId: string, photoId: string): Promise<FinalizedPhoto>;
  block(grant: UploadGrant, blockId: string, body: Blob): Promise<void>;
  commit(grant: UploadGrant, blockIds: string[]): Promise<void>;
}
