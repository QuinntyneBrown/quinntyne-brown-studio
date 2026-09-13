import { Inquiry } from '@qbs/domain/models';
export interface IInquiryInboxService {
  readonly filter: import('@angular/core').WritableSignal<string>;
  readonly busy: import('@angular/core').Signal<boolean>;
  readonly loading: import('@angular/core').Signal<boolean>;
  readonly loadFailed: import('@angular/core').Signal<boolean>;
  inquiries: import('@angular/core').WritableSignal<Inquiry[]>;
  selected: import('@angular/core').WritableSignal<Inquiry | null>;
  message: import('@angular/core').WritableSignal<string>;
  error: import('@angular/core').WritableSignal<boolean>;
  initialize(): void;
  load(): Promise<void>;
  review(): Promise<void>;
  interestLabel(interest: string): string;
}
