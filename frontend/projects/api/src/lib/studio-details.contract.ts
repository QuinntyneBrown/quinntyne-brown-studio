import { StudioDetails } from '@qbs/domain/models';
export interface IStudioDetailsService {
  get(): Promise<StudioDetails>;
  save(value: StudioDetails): Promise<StudioDetails>;
}
