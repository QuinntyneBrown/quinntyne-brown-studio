import { Component, computed, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { SITE } from '../site.token';
import { ACCOUNT_SERVICE } from '../account.token';
import { Notice } from '@qbs/components';
@Component({
  selector: 'qbs-shell',
  imports: [Notice, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrl: './shell.css',
})
export class Shell {
  site = inject(SITE);
  auth = inject(ACCOUNT_SERVICE);
  role = this.site === 'admin' ? 'Administrator' : 'Client';
  authorized = computed(
    () => this.auth.account().authenticated && this.auth.account().roles.includes(this.role),
  );
  /** [label, path, server]: a server-rendered page (OD-13) is reached by a full navigation. */
  publicLinks: [string, string, boolean][] = [
    ['Portfolio', '/portfolio', false],
    ['Services', '/services', false],
    ['About', '/about', true],
    ['Blog', '/blog', true],
    ['Prints', '/prints', false],
    ['Packages', '/promotions', false],
    ['Contact', '/contact', true],
  ];
  footerLinks: [string, string, boolean][] = [
    ['About the studio', '/about', true],
    ['Get in touch', '/contact', true],
    ['Our work', '/portfolio', false],
    ['Blog', '/blog', true],
    ['Client access', '/client/login', true],
  ];
  adminLinks = [
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
    ['Studio details', '/studio-details'],
    ['Package promotions', '/promotions'],
    ['Client invitations', '/invitations'],
    ['Print requests', '/print-requests'],
    ['Inquiries', '/inquiries'],
  ];
  clientLinks = [
    ['Your sessions', '/galleries'],
    ['Your albums', '/albums'],
    ['Request prints', '/prints'],
  ];
}
