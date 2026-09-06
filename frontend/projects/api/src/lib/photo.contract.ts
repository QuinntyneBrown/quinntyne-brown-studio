import { PhotoPage, FinalizedPhoto } from '@qbs/domain/models';
export interface IPhotoService {
  list(sessionId: string, cursor?: string | null): Promise<PhotoPage>;
  retry(id: string): Promise<FinalizedPhoto>;
}
