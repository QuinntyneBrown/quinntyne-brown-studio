import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { SITE } from '../site.token';
import { AuthState } from '../auth-state';
@Component({
  selector: 'qbs-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrl: './shell.css',
})
export class Shell {
  site = inject(SITE);
  auth = inject(AuthState);
  private router = inject(Router);
  menu = signal(false);
  publicLinks = [
    ['Portfolio', '/portfolio'],
    ['Services', '/services'],
    ['Prints', '/prints'],
    ['Packages', '/promotions'],
  ];
  adminLinks = [
    ['Studio overview', '/'],
    ['Sessions', '/sessions'],
    ['Photographers', '/photographers'],
    ['Equipment', '/equipment'],
    ['Preferred vendors', '/vendors'],
    ['Quote rates', '/rates'],
    ['Studios', '/studios'],
    ['Discount rules', '/discounts'],
    ['Print pricing', '/print-options'],
    ['Public galleries', '/public-galleries'],
    ['Website content', '/content'],
    ['Package promotions', '/promotions'],
    ['Client invitations', '/invitations'],
    ['Print requests', '/print-requests'],
  ];
  clientLinks = [
    ['Your sessions', '/galleries'],
    ['Your albums', '/albums'],
    ['Request prints', '/prints'],
  ];
  toggleMenu() {
    this.menu.update((value) => !value);
  }
  async logout() {
    await this.auth.logout();
    await this.router.navigateByUrl('/login');
  }
}
