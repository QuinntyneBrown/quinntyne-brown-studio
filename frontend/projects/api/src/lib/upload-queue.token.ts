import { InjectionToken } from '@angular/core';
import { IUploadQueueService } from './upload-queue.contract';
export const UPLOAD_QUEUE_SERVICE = new InjectionToken<IUploadQueueService>('UPLOAD_QUEUE_SERVICE');
