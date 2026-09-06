import { Signal } from '@angular/core';
import { AccountSession } from '@qbs/domain';
export interface IAccountService {
  readonly menu: import('@angular/core').WritableSignal<boolean>;
  toggleMenu(): void;
  readonly account: Signal<AccountSession>;
  readonly message: Signal<string>;
  readonly busy: Signal<boolean>;
  load(): Promise<AccountSession>;
  logout(): Promise<void>;
}
