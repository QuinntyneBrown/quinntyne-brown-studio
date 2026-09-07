import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { QUOTE_SETTINGS_SERVICE } from '@qbs/api';
import { StudioQuoteSettings } from '@qbs/domain';
import { QuoteSettingsService } from './quote-settings-service';
@Component({
  selector: 'qbs-quote-settings-page',
  imports: [StudioQuoteSettings],
  providers: [{ provide: QUOTE_SETTINGS_SERVICE, useClass: QuoteSettingsService }],
  templateUrl: './quote-settings-page.html',
  styleUrl: './quote-settings-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuoteSettingsPage {
  readonly state = inject(QUOTE_SETTINGS_SERVICE);
}
