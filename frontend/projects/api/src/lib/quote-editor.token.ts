import { InjectionToken } from '@angular/core';
import type { IQuoteEditorService } from './quote-editor.contract';
export const QUOTE_EDITOR_SERVICE = new InjectionToken<IQuoteEditorService>('QUOTE_EDITOR_SERVICE');
