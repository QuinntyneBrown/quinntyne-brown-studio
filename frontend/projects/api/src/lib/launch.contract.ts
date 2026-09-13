import { LaunchState } from '@qbs/domain/models';
export interface ILaunchService {
  state(): Promise<LaunchState>;
}
