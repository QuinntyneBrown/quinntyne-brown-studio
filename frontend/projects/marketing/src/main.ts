import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideBrowserGlobalErrorListeners } from '@angular/core';
import { Shell, SITE, studioRoutes, studioProviders } from '@qbs/application';
bootstrapApplication(Shell, {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(studioRoutes('marketing')),
    { provide: SITE, useValue: 'marketing' },
    ...studioProviders(false),
  ],
}).catch(console.error);
