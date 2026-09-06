import { WritableSignal } from '@angular/core';
import { ITorontoTimeService } from './toronto-time.contract';
import {
  PhotoView,
  StudioSession,
  ClientAccount,
  RetentionImpact,
  AnalysisBatch,
} from '@qbs/domain/models';
export interface ISessionPageService {
  readonly loading: import('@angular/core').Signal<boolean>;
  readonly loadFailed: import('@angular/core').Signal<boolean>;
  readonly photoLoadFailed: import('@angular/core').Signal<boolean>;
  readonly busy: import('@angular/core').Signal<boolean>;
  readonly moreBusy: import('@angular/core').Signal<boolean>;
  photoName(id: string): string;
  hasFailedAnalysis(): boolean;
  readonly time: ITorontoTimeService;
  id: string;
  session: import('@angular/core').WritableSignal<StudioSession | null>;
  photos: import('@angular/core').WritableSignal<PhotoView[]>;
  people: import('@angular/core').WritableSignal<ClientAccount[]>;
  selected: import('@angular/core').WritableSignal<string[]>;
  assignments: WritableSignal<string[]>;
  opened: import('@angular/core').WritableSignal<PhotoView | null>;
  message: import('@angular/core').WritableSignal<string>;
  error: import('@angular/core').WritableSignal<boolean>;
  retention: import('@angular/core').WritableSignal<RetentionImpact | null>;
  analysis: import('@angular/core').WritableSignal<AnalysisBatch | null>;
  nextCursor: import('@angular/core').WritableSignal<string | null>;
  upload: import('./upload-queue.contract').IUploadQueueService;
  months: WritableSignal<number>;
  extension: WritableSignal<string>;
  confirming: import('@angular/core').WritableSignal<boolean>;
  initialize(): Promise<void>;
  ngOnDestroy(): void;
  load(): Promise<void>;
  refresh(): Promise<void>;
  more(): Promise<void>;
  toggle(id: string): void;
  toggleClient(id: string): void;
  saveAssignments(): Promise<void>;
  files(event: Event): Promise<void>;
  analyze(): Promise<void>;
  retryPhoto(id: string): Promise<void>;
  retryAnalysis(): Promise<void>;
  extend(): Promise<void>;
  deletion(): Promise<void>;
  action(fn: () => Promise<unknown>, success: string): Promise<void>;
  fail(e: unknown): void;
}
