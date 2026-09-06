import { Injectable, inject } from '@angular/core';
import { StudioSession, ClientAccount, ClientGallery } from '@qbs/domain/models';
import { IClientGalleryService } from './client-gallery.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class ClientGalleryService implements IClientGalleryService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<ClientGallery[]> {
    return this.transport.get<ClientGallery[]>('client/galleries');
  }
  get(id: string): Promise<ClientGallery> {
    return this.transport.get<ClientGallery>('client/galleries/' + encodeURIComponent(id));
  }
  clients(): Promise<ClientAccount[]> {
    return this.transport.get<ClientAccount[]>('admin/clients');
  }
  invite(email: string): Promise<{
    invitationId: string;
  }> {
    return this.transport.send<{
      invitationId: string;
    }>('POST', 'admin/invitations', { email });
  }
  assign(sessionId: string, clientIds: string[], expectedVersion: number): Promise<StudioSession> {
    return this.transport.send<StudioSession>(
      'PUT',
      `admin/sessions/${encodeURIComponent(sessionId)}/clients`,
      { clientIds, expectedVersion },
    );
  }
}
