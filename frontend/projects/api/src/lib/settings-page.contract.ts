import { WritableSignal } from '@angular/core';
import { ITorontoTimeService } from './toronto-time.contract';
import { SettingsDraft } from '@qbs/domain/models';
import { SettingsRecord } from '@qbs/domain/models';
export interface ISettingsPageService {
  readonly loadFailed: import('@angular/core').Signal<boolean>;
  readonly time: ITorontoTimeService;
  kind: import('@angular/core').WritableSignal<string>;
  title: import('@angular/core').WritableSignal<string>;
  message: import('@angular/core').WritableSignal<string>;
  error: import('@angular/core').WritableSignal<boolean>;
  loading: import('@angular/core').WritableSignal<boolean>;
  busy: import('@angular/core').WritableSignal<boolean>;
  items: import('@angular/core').WritableSignal<SettingsRecord[]>;
  draft: WritableSignal<SettingsDraft>;
  pageKey: WritableSignal<string>;
  readonly windowGroups: ('workingWindows' | 'unavailableWindows')[];
  initialize(): void;
  load(): Promise<void>;
  newRecord(): void;
  label(item: SettingsRecord): string;
  edit(item: SettingsRecord): void;
  save(): Promise<void>;
  fail(e: unknown): void;
}
