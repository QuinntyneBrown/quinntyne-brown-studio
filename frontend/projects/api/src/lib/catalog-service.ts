import { Injectable, inject } from '@angular/core';
import { StudioRecord } from '@qbs/domain/models';
import { ICatalogService } from './catalog.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class CatalogService implements ICatalogService {
  private readonly transport = inject(STUDIO_CLIENT);
  private resource(key: string) {
    if (
      ![
        'equipment',
        'vendors',
        'photographers',
        'sessions',
        'promotions',
        'print-options',
        'public-galleries',
      ].includes(key)
    )
      throw new Error('Unknown catalog.');
    return 'admin/' + key;
  }
  list(resource: string) {
    return this.transport.get<StudioRecord[]>(this.resource(resource));
  }
  save(resource: string, value: Record<string, unknown>) {
    return this.transport.send<StudioRecord>(
      value['id'] ? 'PUT' : 'POST',
      this.resource(resource) + (value['id'] ? '/' + encodeURIComponent(String(value['id'])) : ''),
      { ...value, expectedVersion: value['version'] ?? 0 },
    );
  }
}
