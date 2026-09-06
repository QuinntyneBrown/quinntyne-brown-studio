import { Editable, Photographer } from '@qbs/domain/models';
export interface IPhotographerService {
  list(): Promise<Photographer[]>;
  get(id: string): Promise<Photographer>;
  save(value: Editable<Photographer>): Promise<Photographer>;
}
