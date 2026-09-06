import { Injectable, inject } from '@angular/core';
import { IAuthService } from './auth.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class AuthService implements IAuthService {
  private readonly transport = inject(STUDIO_CLIENT);
  session() {
    return this.transport.session();
  }
  login(email: string, password: string) {
    return this.transport.send<void>('POST', 'auth/login', { email, password });
  }
  logout() {
    return this.transport.send<void>('POST', 'auth/logout', {});
  }
  recover(email: string) {
    return this.transport.send<void>('POST', 'auth/recovery', { email });
  }
  acceptInvitation(token: string, password: string) {
    return this.transport.send<void>('POST', 'auth/accept-invitation', { token, password });
  }
  resetPassword(token: string, password: string) {
    return this.transport.send<void>('POST', 'auth/reset-password', { token, password });
  }
}
