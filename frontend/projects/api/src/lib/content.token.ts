import { InjectionToken } from '@angular/core';
import { IContentService } from './content.contract';
export const CONTENT_SERVICE = new InjectionToken<IContentService>('CONTENT_SERVICE');
