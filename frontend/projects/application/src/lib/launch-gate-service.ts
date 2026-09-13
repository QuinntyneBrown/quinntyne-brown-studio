import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';
import { LAUNCH_SERVICE } from '@qbs/api';
import { ILaunchGateService } from './launch-gate.contract';
@Injectable()
export class LaunchGateService implements ILaunchGateService {
  private readonly api = inject(LAUNCH_SERVICE);
  private readonly document = inject(DOCUMENT);
  private pending: Promise<boolean> | null = null;
  readonly comingSoon = signal(false);
  load() {
    // One read serves the shell and every guard; a state the API cannot supply gates nobody,
    // because the blog the gate would send visitors to comes from the same API.
    this.pending ??= this.api
      .state()
      .then((state) => state.comingSoon)
      .catch(() => false)
      .then((comingSoon) => {
        this.comingSoon.set(comingSoon);
        return comingSoon;
      });
    return this.pending;
  }
  leave() {
    this.document.location.assign('/blog');
  }
}
