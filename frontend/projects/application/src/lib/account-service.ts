import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AUTH_SERVICE } from '@qbs/api';
import { AccountSession } from '@qbs/domain';
import { IAccountService } from './account.contract';
@Injectable()
export class AccountService implements IAccountService {
  readonly menu = signal(false);
  toggleMenu() {
    this.menu.update((value) => !value);
  }
  private readonly api = inject(AUTH_SERVICE);
  private readonly router = inject(Router);
  readonly account = signal<AccountSession>({ authenticated: false, id: null, roles: [] });
  readonly message = signal('');
  readonly busy = signal(false);
  async load() {
    this.account.set(await this.api.session());
    return this.account();
  }
  async logout() {
    if (this.busy()) return;
    this.busy.set(true);
    this.message.set('');
    try {
      await this.api.logout();
      this.account.set({ authenticated: false, id: null, roles: [] });
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
