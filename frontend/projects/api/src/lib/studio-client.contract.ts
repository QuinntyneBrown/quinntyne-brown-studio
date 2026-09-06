import { AccountSession } from '@qbs/domain';
export interface IStudioClient {
  get<T>(path: string): Promise<T>;
  send<T>(method: 'POST' | 'PUT', path: string, body: unknown): Promise<T>;
  session(): Promise<AccountSession>;
  upload(url: string, blockId: string | undefined, body: Blob | string): Promise<void>;
}
