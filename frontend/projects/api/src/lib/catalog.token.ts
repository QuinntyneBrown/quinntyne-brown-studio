import { InjectionToken } from '@angular/core';
import { ICatalogService } from './catalog.contract';
export const CATALOG_SERVICE = new InjectionToken<ICatalogService>('CATALOG_SERVICE');
