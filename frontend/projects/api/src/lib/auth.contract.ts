import { AccountSession } from '@qbs/domain/models';
export interface IAuthService {
  session(): Promise<AccountSession>;
  login(email: string, password: string): Promise<void>;
  logout(): Promise<void>;
  recover(email: string): Promise<void>;
  acceptInvitation(token: string, password: string): Promise<void>;
  resetPassword(token: string, password: string): Promise<void>;
}
