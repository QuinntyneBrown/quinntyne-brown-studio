import { inject } from '@angular/core';
import { Routes, CanActivateFn, Router } from '@angular/router';
import { ACCOUNT_SERVICE } from './account.token';
import { PublicPage } from './public-page/public-page';
import { QuotePage } from './quote-page/quote-page';
import { CatalogPage } from './catalog-page/catalog-page';
import { SettingsPage } from './settings-page/settings-page';
import { SessionPage } from './session-page/session-page';
import { ClientPage } from './client-page/client-page';
import { LoginPage } from './login-page/login-page';
import { PrintInbox } from './print-inbox/print-inbox';
import { RESOURCES } from './resource-definitions';
const access: CanActivateFn = async (route) => {
  const auth = inject(ACCOUNT_SERVICE),
    router = inject(Router);
  try {
    const account = await auth.load();
    return account.authenticated && account.roles.includes(route.data['role'])
      ? true
      : router.createUrlTree(['/login']);
  } catch {
    return router.createUrlTree(['/login']);
  }
};
const accountRoutes: Routes = [
  { path: 'login', component: LoginPage, data: { mode: 'login' } },
  { path: 'forgot-password', component: LoginPage, data: { mode: 'recovery' } },
  { path: 'reset-password', component: LoginPage, data: { mode: 'reset-password' } },
  { path: 'accept-invitation', component: LoginPage, data: { mode: 'accept-invitation' } },
];
export function studioRoutes(site: string): Routes {
  if (site === 'marketing')
    return [
      { path: '', component: PublicPage, data: { kind: 'home' } },
      ...['portfolio', 'services', 'prints', 'promotions', 'contact'].map((kind) => ({
        path: kind,
        component: PublicPage,
        data: { kind },
      })),
      { path: 'galleries/:slug', component: PublicPage, data: { kind: 'gallery' } },
      { path: 'quote', component: QuotePage },
      { path: '**', redirectTo: '' },
    ];
  const role = site === 'admin' ? 'Administrator' : 'Client';
  const features: Routes =
    site === 'admin'
      ? [
          { path: '', redirectTo: 'sessions', pathMatch: 'full' },
          ...RESOURCES.map((r) => ({
            path: r.key,
            component: CatalogPage,
            data: { resource: r.key, role },
            canActivate: [access],
          })),
          ...['rates', 'discounts', 'studios', 'content', 'invitations'].map((kind) => ({
            path: kind,
            component: SettingsPage,
            data: { kind, role },
            canActivate: [access],
          })),
          {
            path: 'schedule/:id',
            component: SettingsPage,
            data: { kind: 'schedule', role },
            canActivate: [access],
          },
          { path: 'sessions/:id', component: SessionPage, data: { role }, canActivate: [access] },
          { path: 'print-requests', component: PrintInbox, data: { role }, canActivate: [access] },
        ]
      : [
          { path: '', redirectTo: 'galleries', pathMatch: 'full' },
          ...['galleries', 'albums', 'prints'].map((kind) => ({
            path: kind,
            component: ClientPage,
            data: { kind, role },
            canActivate: [access],
          })),
          {
            path: 'galleries/:id',
            component: ClientPage,
            data: { kind: 'gallery', role },
            canActivate: [access],
          },
          {
            path: 'albums/:id',
            component: ClientPage,
            data: { kind: 'album', role },
            canActivate: [access],
          },
        ];
  return [...accountRoutes, ...features, { path: '**', redirectTo: '' }];
}
