import { Editable, Vendor } from '@qbs/domain/models';
export interface IVendorService {
  list(): Promise<Vendor[]>;
  get(id: string): Promise<Vendor>;
  save(value: Editable<Vendor>): Promise<Vendor>;
}
