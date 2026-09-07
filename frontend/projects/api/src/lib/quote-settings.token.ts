import { InjectionToken } from '@angular/core';
import { IQuoteSettingsService } from './quote-settings.contract';
export const QUOTE_SETTINGS_SERVICE = new InjectionToken<IQuoteSettingsService>('Quote settings');
