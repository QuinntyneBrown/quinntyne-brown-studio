import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { STUDIO_API } from '@qbs/api';
import { Notice } from '@qbs/components';
import { SITE } from '../site.token';
import { AuthState } from '../auth-state';
@Component({
  selector: 'qbs-login-page',
  imports: [FormsModule, RouterLink, Notice],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
})
export class LoginPage {
  private api = inject(STUDIO_API);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private auth = inject(AuthState);
  site = inject(SITE);
  mode = this.route.snapshot.data['mode'] ?? 'login';
  email = '';
  password = '';
  message = signal('');
  error = signal(false);
  busy = signal(false);
  get title() {
    return this.mode === 'login'
      ? 'Welcome back.'
      : this.mode === 'recovery'
        ? 'Find your way back.'
        : this.mode === 'accept-invitation'
          ? 'Your photographs await.'
          : 'A fresh start.';
  }
  async submit() {
    this.busy.set(true);
    try {
      const body =
        this.mode === 'login'
          ? { email: this.email, password: this.password }
          : this.mode === 'recovery'
            ? { email: this.email }
            : {
                token: this.route.snapshot.queryParamMap.get('token') ?? '',
                password: this.password,
              };
      await this.api.send('POST', 'auth/' + this.mode, body);
      this.error.set(false);
      if (this.mode === 'login' || this.mode === 'accept-invitation') {
        await this.auth.load();
        await this.router.navigateByUrl(this.site === 'admin' ? '/sessions' : '/galleries');
      } else
        this.message.set(
          this.mode === 'recovery'
            ? 'If the account is eligible, recovery instructions will be sent.'
            : 'Password updated. You can sign in now.',
        );
    } catch (e) {
      this.error.set(true);
      this.message.set(e instanceof Error ? e.message : 'Unable to continue.');
    } finally {
      this.busy.set(false);
    }
  }
}
