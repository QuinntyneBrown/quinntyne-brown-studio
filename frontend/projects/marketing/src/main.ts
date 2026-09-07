import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideBrowserGlobalErrorListeners } from '@angular/core';
import { Shell, SITE, studioRoutes, studioProviders, reckonerPublicProvider } from '@qbs/application';
import { reckonerPublicOptions } from './environments/reckoner';
bootstrapApplication(Shell, {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(studioRoutes('marketing')),
    { provide: SITE, useValue: 'marketing' },
    reckonerPublicProvider(reckonerPublicOptions),
    ...studioProviders(),
  ],
}).catch(console.error);
