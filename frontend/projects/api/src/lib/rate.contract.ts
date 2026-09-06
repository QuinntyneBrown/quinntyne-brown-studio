import { RateConfiguration } from '@qbs/domain/models';
export interface IRateService {
  get(): Promise<RateConfiguration>;
  save(value: RateConfiguration): Promise<RateConfiguration>;
}
