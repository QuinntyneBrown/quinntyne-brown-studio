import { Injectable, inject } from '@angular/core';
import { UploadManifestEntry, UploadBatch, UploadGrant, FinalizedPhoto } from '@qbs/domain/models';
import { IUploadService } from './upload.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class UploadService implements IUploadService {
  private readonly transport = inject(STUDIO_CLIENT);
  create(sessionId: string, files: UploadManifestEntry[]): Promise<UploadBatch> {
    return this.transport.send<UploadBatch>(
      'POST',
      `admin/sessions/${encodeURIComponent(sessionId)}/uploads`,
      { files },
    );
  }
  status(id: string): Promise<UploadBatch> {
    return this.transport.get<UploadBatch>('admin/uploads/' + encodeURIComponent(id));
  }
  renew(batchId: string, photoId: string): Promise<UploadGrant> {
    return this.transport.send<UploadGrant>(
      'POST',
      `admin/uploads/${encodeURIComponent(batchId)}/files/${encodeURIComponent(photoId)}/renew`,
      {},
    );
  }
  complete(batchId: string, photoId: string): Promise<FinalizedPhoto> {
    return this.transport.send<FinalizedPhoto>(
      'POST',
      `admin/uploads/${encodeURIComponent(batchId)}/files/${encodeURIComponent(photoId)}/complete`,
      {},
    );
  }
  block(grant: UploadGrant, blockId: string, body: Blob): Promise<void> {
    return this.transport.upload(grant.url, blockId, body);
  }
  commit(grant: UploadGrant, blockIds: string[]): Promise<void> {
    return this.transport.upload(
      grant.url,
      undefined,
      '<?xml version="1.0" encoding="utf-8"?><BlockList>' +
        blockIds.map((id) => '<Latest>' + id + '</Latest>').join('') +
        '</BlockList>',
    );
  }
}
