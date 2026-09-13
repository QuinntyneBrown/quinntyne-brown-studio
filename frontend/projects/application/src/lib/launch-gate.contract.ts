import { Signal } from '@angular/core';
/**
 * The relaunch gate (OD-14): while the studio is coming soon, the client-rendered marketing
 * pages stay behind the blog for a visitor who is not signed in.
 */
export interface ILaunchGateService {
  /** Whether the gate applies to this visitor; false until the state has loaded. */
  readonly comingSoon: Signal<boolean>;
  /** Loads the state once per page load and resolves to whether the gate applies. */
  load(): Promise<boolean>;
  /** Leaves the application for the blog, the one public page a gated visitor is sent to. */
  leave(): void;
}
