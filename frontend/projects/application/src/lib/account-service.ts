import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AUTH_SERVICE } from '@qbs/api';
import { AccountSession } from '@qbs/domain';
import { IAccountService } from './account.contract';
import { SITE } from './site.token';
@Injectable()
export class AccountService implements IAccountService {
  readonly menu = signal(false);
  toggleMenu() {
    this.menu.update((value) => !value);
  }
  private readonly api = inject(AUTH_SERVICE);
  private readonly router = inject(Router);
  private readonly site = inject(SITE);
  readonly account = signal<AccountSession>({ authenticated: false, id: null, roles: [] });
  readonly canAccessWorkspace = computed(() => {
    const account = this.account();
    const role = this.site === 'admin' ? 'Administrator' : 'Client';
    return this.site !== 'marketing' && account.authenticated && account.roles.includes(role);
  });
  readonly message = signal('');
  readonly busy = signal(false);
  async load() {
    try {
      this.account.set(await this.api.session());
      return this.account();
    } catch (error) {
      this.account.set({ authenticated: false, id: null, roles: [] });
      throw error;
    }
  }
  async logout() {
    if (this.busy()) return;
    this.busy.set(true);
    this.message.set('');
    try {
      await this.api.logout();
      this.account.set({ authenticated: false, id: null, roles: [] });
      this.menu.set(false);
      await this.router.navigateByUrl('/login');
    } catch (error) {
      this.message.set(
        error instanceof Error ? error.message : 'Sign out is unavailable. Try again.',
      );
    } finally {
      this.busy.set(false);
    }
  }
}
