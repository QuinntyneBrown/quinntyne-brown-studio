import { Injectable, inject } from '@angular/core';
import { PhotoPage, FinalizedPhoto } from '@qbs/domain/models';
import { IPhotoService } from './photo.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class PhotoService implements IPhotoService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(sessionId: string, cursor?: string | null): Promise<PhotoPage> {
    return this.transport.get<PhotoPage>(
      `admin/sessions/${encodeURIComponent(sessionId)}/photos` +
        (cursor ? '?cursor=' + encodeURIComponent(cursor) : ''),
    );
  }
  retry(id: string): Promise<FinalizedPhoto> {
    return this.transport.send<FinalizedPhoto>(
      'POST',
      `admin/photos/${encodeURIComponent(id)}/retry-preview`,
      {},
    );
  }
}
