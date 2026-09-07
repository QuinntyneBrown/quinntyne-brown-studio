import { ReckonerAdminSession } from './reckoner-admin-session';
export interface IReckonerAdminService {
  mint(signal: AbortSignal): Promise<ReckonerAdminSession>;
}
