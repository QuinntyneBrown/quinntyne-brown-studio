import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideBrowserGlobalErrorListeners } from '@angular/core';
import { Shell, SITE, studioRoutes, studioProviders } from '@qbs/application';
bootstrapApplication(Shell, {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(studioRoutes('design-system')),
    { provide: SITE, useValue: 'design-system' },
    ...studioProviders(true),
  ],
}).catch(console.error);
