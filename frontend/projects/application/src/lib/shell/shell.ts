import { Component, inject, signal } from '@angular/core';
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
  publicLinks = [
    ['Portfolio', '/portfolio'],
    ['Services', '/services'],
    ['Prints', '/prints'],
    ['Packages', '/promotions'],
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
    ['Package promotions', '/promotions'],
    ['Client invitations', '/invitations'],
    ['Print requests', '/print-requests'],
  ];
  clientLinks = [
    ['Your sessions', '/galleries'],
    ['Your albums', '/albums'],
    ['Request prints', '/prints'],
  ];
}
