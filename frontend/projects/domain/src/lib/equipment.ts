export interface Equipment {
  id: string;
  version: number;
  name: string;
  description: string;
  quantity: number;
  referenceRentalRate: string | number | null;
}
