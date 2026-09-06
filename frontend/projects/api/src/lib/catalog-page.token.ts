import { InjectionToken } from '@angular/core';
import { ICatalogPageService } from './catalog-page.contract';
export const CATALOG_PAGE_SERVICE = new InjectionToken<ICatalogPageService>('CATALOG_PAGE_SERVICE');
