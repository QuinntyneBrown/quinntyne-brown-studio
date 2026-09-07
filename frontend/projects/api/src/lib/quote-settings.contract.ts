import { Signal } from '@angular/core';
import { ReckonerAdminSession } from './reckoner-admin-session';
export interface IQuoteSettingsService {
  readonly kind: Signal<string>;
  readonly title: Signal<string>;
  readonly session: Signal<ReckonerAdminSession | null>;
  readonly busy: Signal<boolean>;
  readonly error: Signal<string>;
  refresh(): Promise<void>;
}
