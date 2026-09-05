export interface AccountSession {
  authenticated: boolean;
  id: string | null;
  roles: string[];
}
