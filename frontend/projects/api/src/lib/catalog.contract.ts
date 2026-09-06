import { StudioRecord } from '@qbs/domain/models';
export interface ICatalogService {
  list(resource: string): Promise<StudioRecord[]>;
  save(resource: string, value: Record<string, unknown>): Promise<StudioRecord>;
}
