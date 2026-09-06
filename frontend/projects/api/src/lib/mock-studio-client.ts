import { Injectable } from '@angular/core';
import { AccountSession } from '@qbs/domain';
import { IStudioClient } from './studio-client.contract';

/** Deterministic, in-memory catalog data. No credentials or product network access. */
@Injectable()
export class MockStudioClient implements IStudioClient {
  private records = new Map<string, unknown>();
  async get<T>(path: string): Promise<T> {
    const clean = path.split('?')[0];
    if (this.records.has(clean)) return structuredClone(this.records.get(clean)) as T;
    let result: unknown = [];
    if (clean === 'auth/session')
      result = { authenticated: true, id: 'example', roles: ['Administrator', 'Client'] };
    else if (clean.includes('content/'))
      result = {
        heading: 'Photography with feeling.',
        body: 'Honest moments. Thoughtfully captured.',
      };
    else if (clean === 'admin/rates')
      result = {
        version: 0,
        serviceRates: { Wedding: '150', Event: '125', Headshot: '100', FamilyPortrait: '100' },
        costRates: { travel: '0.75', equipment: '25', lunch: '20', assistant: '35' },
      };
    else if (clean.endsWith('/retention'))
      result = {
        months: 12,
        version: 1,
        state: 'Active',
        photoCount: 0,
        publishedReferences: 0,
        unreviewedRequests: 0,
        impactRevision: 'example-revision',
        expiresAt: null,
      };
    else if (clean.endsWith('/photos')) result = { photos: [], nextCursor: null };
    else if (/^admin\/sessions\/[^/]+$/.test(clean))
      result = {
        id: 'example-session',
        name: 'An afternoon together',
        service: 'FamilyPortrait',
        version: 1,
        clientIds: [],
        startsAt: '2027-06-01T14:00:00-04:00',
        endsAt: '2027-06-01T15:00:00-04:00',
      };
    return structuredClone(result) as T;
  }
  async send<T>(method: 'POST' | 'PUT', path: string, body: unknown): Promise<T> {
    if (path === 'public/locations/resolve')
      return [{ label: 'Example studio, Toronto', latitude: 43.65, longitude: -79.38 }] as T;
    if (path === 'public/quotes/calculate') {
      const input = body as { inputRevision: number };
      const money = { amount: '150.00', currency: 'CAD' };
      return {
        inputRevision: input.inputRevision,
        configurationRevision: 1,
        lines: [
          {
            kind: 'Illustrative session',
            locationIndex: null,
            quantity: '1',
            unitRate: money,
            amount: money,
          },
        ],
        subtotal: money,
        discount: {
          percentage: '0',
          amount: { amount: '0', currency: 'CAD' },
          kind: null,
          codeError: null,
        },
        total: money,
        availability: { available: true, photographerIds: [], reasonCode: null },
      } as T;
    }
    const value = { ...(body as object), id: 'example', version: 1 };
    if (method === 'POST') this.records.set(path, [...(await this.get<object[]>(path)), value]);
    else this.records.set(path, value);
    return structuredClone(value) as T;
  }
  session() {
    return this.get<AccountSession>('auth/session');
  }
  async upload(_url: string, _block: string | undefined, _body: Blob | string) {}
}
