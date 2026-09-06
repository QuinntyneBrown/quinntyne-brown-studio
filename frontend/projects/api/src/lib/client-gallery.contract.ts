import { StudioSession, ClientAccount, ClientGallery } from '@qbs/domain/models';
export interface IClientGalleryService {
  list(): Promise<ClientGallery[]>;
  get(id: string): Promise<ClientGallery>;
  clients(): Promise<ClientAccount[]>;
  invite(email: string): Promise<{
    invitationId: string;
  }>;
  assign(sessionId: string, clientIds: string[], expectedVersion: number): Promise<StudioSession>;
}
