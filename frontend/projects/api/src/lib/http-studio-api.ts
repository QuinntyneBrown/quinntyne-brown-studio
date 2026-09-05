import { Injectable } from '@angular/core';
import { AccountSession, ApiError } from '@qbs/domain';
import { IStudioApi } from './studio-api.contract';
@Injectable()
export class HttpStudioApi implements IStudioApi {
  private token: string | null = null;
  private async result<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new ApiError(
        response.status,
        error.title || 'The service is unavailable. Please try again.',
        error.errors || {},
      );
    }
    return response.status === 204 ? (undefined as T) : (response.json() as Promise<T>);
  }
  async get<T>(path: string): Promise<T> {
    return this.result<T>(await fetch('/api/' + path, { credentials: 'same-origin' }));
  }
  private async csrf() {
    if (!this.token)
      this.token = (await this.get<{ requestToken: string }>('auth/antiforgery')).requestToken;
    return this.token;
  }
  async send<T>(method: 'POST' | 'PUT', path: string, body: unknown): Promise<T> {
    const token = await this.csrf();
    const result = await this.result<T>(
      await fetch('/api/' + path, {
        method,
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json', 'X-XSRF-TOKEN': token },
        body: JSON.stringify(body),
      }),
    );
    if (path.startsWith('auth/')) this.token = null;
    return result;
  }
  session() {
    return this.get<AccountSession>('auth/session');
  }
  async upload(url: string, blockId: string | undefined, body: Blob | string) {
    const target = new URL(url, location.origin);
    target.searchParams.set('comp', blockId ? 'block' : 'blocklist');
    if (blockId) target.searchParams.set('blockid', blockId);
    const headers: Record<string, string> = {
      'x-ms-version': '2023-11-03',
      'Content-Type': blockId ? 'application/octet-stream' : 'application/xml',
    };
    if (target.origin === location.origin) headers['X-XSRF-TOKEN'] = await this.csrf();
    const r = await fetch(target, {
      method: 'PUT',
      headers,
      body,
      credentials: target.origin === location.origin ? 'same-origin' : 'omit',
    });
    if (!r.ok)
      throw new ApiError(r.status, 'Upload interrupted. Reselect the same files to resume.');
  }
}
