import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { LAUNCH_GATE_SERVICE } from './launch-gate.token';
/** A client-rendered marketing page opens only when the relaunch gate does not apply (OD-14). */
export const launchGate: CanActivateFn = async () => {
  const gate = inject(LAUNCH_GATE_SERVICE);
  if (!(await gate.load())) return true;
  gate.leave();
  return false;
};
