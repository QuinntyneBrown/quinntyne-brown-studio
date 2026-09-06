import { Signal, WritableSignal } from '@angular/core';
export interface ILoginService {
  readonly mode: string;
  readonly title: string;
  readonly email: WritableSignal<string>;
  readonly password: WritableSignal<string>;
  readonly message: Signal<string>;
  readonly error: Signal<boolean>;
  readonly busy: Signal<boolean>;
  submit(): Promise<void>;
}
