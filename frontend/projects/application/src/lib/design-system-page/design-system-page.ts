import { Component, signal } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { Notice, EmptyState, PhotoGrid, Dialog } from '@qbs/components';
import { CatalogPage } from '../catalog-page/catalog-page';
import { QuotePage } from '../quote-page/quote-page';
import { PublicPage } from '../public-page/public-page';
import { ClientPage } from '../client-page/client-page';
import { LoginPage } from '../login-page/login-page';
import { SettingsPage } from '../settings-page/settings-page';
import { SessionPage } from '../session-page/session-page';
import { PrintInbox } from '../print-inbox/print-inbox';
import { inject } from '@angular/core';
import { COMPONENT_CONTRACTS } from '../component-contracts';
@Component({
  selector: 'qbs-design-system',
  imports: [
    RouterLink,
    Notice,
    EmptyState,
    PhotoGrid,
    Dialog,
    CatalogPage,
    QuotePage,
    PublicPage,
    ClientPage,
    LoginPage,
    SettingsPage,
    SessionPage,
    PrintInbox,
  ],
  templateUrl: './design-system-page.html',
  styleUrl: './design-system-page.css',
})
export class DesignSystemPage {
  private route = inject(ActivatedRoute);
  dialog = signal(false);
  example = signal('buttons');
  state = signal('success');
  entries = [
    'Buttons',
    'Typography',
    'Forms',
    'Notice',
    'Empty state',
    'Photo grid',
    'Dialog',
    'Catalog page',
    'Quote page',
    'Public page',
    'Client page',
    'Login page',
    'Settings page',
    'Session page',
    'Print inbox',
    'Shell',
    'Design system',
  ];
  constructor() {
    this.route.paramMap.subscribe((p) => this.example.set(p.get('example') ?? 'buttons'));
    this.route.queryParamMap.subscribe((p) => this.state.set(p.get('state') ?? 'success'));
  }
  openDialog(event: Event) {
    (event.currentTarget as HTMLElement).focus();
    this.dialog.set(true);
  }
  slug(label: string) {
    return label.toLowerCase().replaceAll(' ', '-');
  }
  contract() {
    return COMPONENT_CONTRACTS.find((entry) => entry.route === '/components/' + this.example());
  }
}
