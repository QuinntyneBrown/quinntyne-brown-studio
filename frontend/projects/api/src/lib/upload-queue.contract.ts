import { Signal } from '@angular/core';
import { UploadRow } from '@qbs/domain/models';
export interface IUploadQueueService {
  readonly rows: Signal<UploadRow[]>;
  readonly busy: Signal<boolean>;
  readonly batchId: Signal<string | null>;
  readonly warning: Signal<string>;
  start(session: string, files: FileList): Promise<void>;
  refreshStatus(): Promise<void>;
}
