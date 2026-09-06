import { Editable, StudioSession } from '@qbs/domain/models';
export interface ISessionService {
  list(): Promise<StudioSession[]>;
  get(id: string): Promise<StudioSession>;
  save(value: Editable<StudioSession>): Promise<StudioSession>;
}
