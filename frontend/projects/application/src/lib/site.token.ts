import { InjectionToken } from '@angular/core';
export const SITE = new InjectionToken<'marketing' | 'admin' | 'client' | 'design-system'>('SITE');
