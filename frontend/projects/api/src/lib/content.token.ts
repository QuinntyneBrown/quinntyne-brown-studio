import { InjectionToken } from '@angular/core';
import { IContentApi } from './content.contract';
export const CONTENT_API = new InjectionToken<IContentApi>('ContentApi');
