import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AUTH_SERVICE } from '@qbs/api';
import { ACCOUNT_SERVICE } from '../account.token';
import { SITE } from '../site.token';
import { ILoginService } from './login.contract';
@Injectable()
export class LoginService implements ILoginService {
  private readonly api = inject(AUTH_SERVICE);
  private readonly account = inject(ACCOUNT_SERVICE);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly site = inject(SITE);
  readonly mode: string = this.route.snapshot.data['mode'] ?? 'login';
  readonly title =
    this.mode === 'login'
      ? 'Welcome back.'
      : this.mode === 'recovery'
        ? 'Find your way back.'
        : this.mode === 'accept-invitation'
          ? 'Your photographs await.'
          : 'A fresh start.';
  readonly email = signal('');
  readonly password = signal('');
  readonly message = signal('');
  readonly error = signal(false);
  readonly busy = signal(false);
  async submit() {
    if (this.busy()) return;
    this.busy.set(true);
    this.message.set('');
    this.error.set(false);
    try {
      const token = this.route.snapshot.queryParamMap.get('token')?.trim();
      if ((this.mode === 'reset-password' || this.mode === 'accept-invitation') && !token)
        throw new Error('This link is incomplete. Open the complete link from your email.');
      if (this.mode === 'login') await this.api.login(this.email().trim(), this.password());
      else if (this.mode === 'recovery') await this.api.recover(this.email().trim());
      else if (this.mode === 'accept-invitation')
        await this.api.acceptInvitation(token!, this.password());
      else await this.api.resetPassword(token!, this.password());
      if (this.mode === 'login' || this.mode === 'accept-invitation') {
        const account = await this.account.load();
        const role = this.site === 'admin' ? 'Administrator' : 'Client';
        if (!account.authenticated || !account.roles.includes(role))
          throw new Error(
            this.site === 'admin'
              ? 'This account does not have access to studio administration.'
              : 'This account does not have access to client collections.',
          );
        this.password.set('');
        await this.router.navigateByUrl(this.site === 'admin' ? '/sessions' : '/galleries');
      } else {
        this.password.set('');
        this.message.set(
          this.mode === 'recovery'
            ? 'If the account is eligible, recovery instructions will be sent.'
            : 'Password updated. You can sign in now.',
        );
      }
    } catch (error) {
      this.error.set(true);
      this.message.set(error instanceof Error ? error.message : 'Unable to continue. Try again.');
    } finally {
      this.busy.set(false);
    }
  }
}
