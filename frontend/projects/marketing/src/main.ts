import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideBrowserGlobalErrorListeners } from '@angular/core';
import { Shell, SITE, studioRoutes, studioProviders, reckonerPublicProvider } from '@qbs/application';
import { reckonerPublicOptions } from './environments/reckoner';
bootstrapApplication(Shell, {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      studioRoutes('marketing'),
      withInMemoryScrolling({ scrollPositionRestoration: 'enabled', anchorScrolling: 'enabled' }),
    ),
    { provide: SITE, useValue: 'marketing' },
    reckonerPublicProvider(reckonerPublicOptions),
    ...studioProviders(),
  ],
}).catch(console.error);
