import { Injectable, inject } from '@angular/core';
import { LaunchState } from '@qbs/domain/models';
import { ILaunchService } from './launch.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class LaunchService implements ILaunchService {
  private readonly transport = inject(STUDIO_CLIENT);
  state(): Promise<LaunchState> {
    return this.transport.get<LaunchState>('public/launch');
  }
}
