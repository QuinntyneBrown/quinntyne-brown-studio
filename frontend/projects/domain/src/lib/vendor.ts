export interface Vendor {
  id: string;
  version: number;
  name: string;
  email: string | null;
  phone: string | null;
  roles: string[];
}
