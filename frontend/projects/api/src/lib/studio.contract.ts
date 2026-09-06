import { Editable, StudioConfiguration } from '@qbs/domain/models';
export interface IStudioService {
  list(): Promise<StudioConfiguration[]>;
  get(id: string): Promise<StudioConfiguration>;
  save(value: Editable<StudioConfiguration>): Promise<StudioConfiguration>;
}
