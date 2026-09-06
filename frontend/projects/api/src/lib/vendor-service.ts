import { Injectable, inject } from '@angular/core';
import { Editable, Vendor } from '@qbs/domain/models';
import { IVendorService } from './vendor.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class VendorService implements IVendorService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<Vendor[]> {
    return this.transport.get<Vendor[]>('admin/vendors');
  }
  get(id: string): Promise<Vendor> {
    return this.transport.get<Vendor>(`admin/vendors/${encodeURIComponent(id)}`);
  }
  save(value: Editable<Vendor>): Promise<Vendor> {
    return this.transport.send<Vendor>(
      value.id ? 'PUT' : 'POST',
      'admin/vendors' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
}
