import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Notice } from '@qbs/components';
import { QUOTE_EDITOR_SERVICE } from '../quote-editor.token';
@Component({
  selector: 'qbs-quote-summary',
  imports: [Notice],
  templateUrl: './quote-summary.html',
  styleUrl: './quote-summary.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuoteSummary {
  readonly editor = inject(QUOTE_EDITOR_SERVICE);
  readonly labels: Record<string, string> = {
    photography: 'Photography',
    travel: 'Travel',
    equipment: 'Equipment',
    lunch: 'Lunches',
    assistant: 'Assistants',
    parking: 'Parking',
    studio: 'Studio',
  };
  readonly discounts: Record<string, string> = {
    Code: 'Discount code',
    Advance: 'Advance booking',
    Weekday: 'Weekday discount',
  };
}
