import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, inject } from '@angular/core';
import { QUOTE_SETTINGS_SERVICE } from '@qbs/api';
import { defineAdminElements } from 'reckoner/admin';
@Component({
  selector: 'qbs-studio-quote-settings',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './studio-quote-settings.html',
  styleUrl: './studio-quote-settings.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudioQuoteSettings {
  readonly state = inject(QUOTE_SETTINGS_SERVICE);
  constructor() {
    defineAdminElements();
  }
}
