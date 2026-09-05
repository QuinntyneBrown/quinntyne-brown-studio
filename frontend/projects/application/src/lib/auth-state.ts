import { Injectable, inject, signal } from '@angular/core';
import { STUDIO_API } from '@qbs/api';
import { AccountSession } from '@qbs/domain';
@Injectable({ providedIn: 'root' })
export class AuthState {
  private api = inject(STUDIO_API);
  account = signal<AccountSession>({ authenticated: false, id: null, roles: [] });
  ready = false;
  async load() {
    this.account.set(await this.api.session());
    this.ready = true;
    return this.account();
  }
  async logout() {
    await this.api.send('POST', 'auth/logout', {});
    this.account.set({ authenticated: false, id: null, roles: [] });
    this.ready = false;
  }
}
