import { InjectionToken } from '@angular/core';
import { IUploadService } from './upload.contract';
export const UPLOAD_SERVICE = new InjectionToken<IUploadService>('UPLOAD_SERVICE');
