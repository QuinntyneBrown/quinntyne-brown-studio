import { PrintRequest } from '@qbs/domain/models';
export interface IPrintInboxService {
  readonly filter: import('@angular/core').WritableSignal<string>;
  readonly busy: import('@angular/core').Signal<boolean>;
  readonly loading: import('@angular/core').Signal<boolean>;
  readonly loadFailed: import('@angular/core').Signal<boolean>;
  requests: import('@angular/core').WritableSignal<PrintRequest[]>;
  selected: import('@angular/core').WritableSignal<PrintRequest | null>;
  message: import('@angular/core').WritableSignal<string>;
  error: import('@angular/core').WritableSignal<boolean>;
  initialize(): void;
  load(): Promise<void>;
  review(): Promise<void>;
}
