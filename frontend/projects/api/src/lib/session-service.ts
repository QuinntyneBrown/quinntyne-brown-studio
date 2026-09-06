import { Injectable, inject } from '@angular/core';
import { Editable, StudioSession } from '@qbs/domain/models';
import { ISessionService } from './session.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class SessionService implements ISessionService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<StudioSession[]> {
    return this.transport.get<StudioSession[]>('admin/sessions');
  }
  get(id: string): Promise<StudioSession> {
    return this.transport.get<StudioSession>(`admin/sessions/${encodeURIComponent(id)}`);
  }
  save(value: Editable<StudioSession>): Promise<StudioSession> {
    return this.transport.send<StudioSession>(
      value.id ? 'PUT' : 'POST',
      'admin/sessions' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
}
