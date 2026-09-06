import { InjectionToken } from '@angular/core';
import { ISettingsPageService } from './settings-page.contract';
export const SETTINGS_PAGE_SERVICE = new InjectionToken<ISettingsPageService>(
  'SETTINGS_PAGE_SERVICE',
);
