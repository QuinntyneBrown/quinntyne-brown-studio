import { inject, Injectable } from '@angular/core';
import { STUDIO_CLIENT } from './studio-client.token';
import { IReckonerAdminService } from './reckoner-admin.contract';
import { ReckonerAdminSession } from './reckoner-admin-session';
@Injectable()
export class ReckonerAdminService implements IReckonerAdminService {
  private readonly client = inject(STUDIO_CLIENT);
  mint(signal: AbortSignal) {
    return this.client.send<ReckonerAdminSession>('POST', 'admin/reckoner/token', {}, signal);
  }
}
