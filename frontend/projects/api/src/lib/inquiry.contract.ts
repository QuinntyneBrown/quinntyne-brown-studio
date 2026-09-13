import { Inquiry } from '@qbs/domain/models';
export interface IInquiryService {
  list(state?: string): Promise<Inquiry[]>;
  get(id: string): Promise<Inquiry>;
  review(id: string, expectedVersion: number): Promise<Inquiry>;
}
