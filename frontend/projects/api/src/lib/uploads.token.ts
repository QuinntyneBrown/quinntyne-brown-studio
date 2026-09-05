import { InjectionToken } from '@angular/core';
import { IUploadsApi } from './uploads.contract';
export const UPLOADS_API = new InjectionToken<IUploadsApi>('UploadsApi');
