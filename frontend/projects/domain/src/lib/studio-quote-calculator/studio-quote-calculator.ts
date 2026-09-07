import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { QUOTE_EDITOR_SERVICE } from '@qbs/api';
import { ReckonerQuoteViewComponent } from 'reckoner/angular/quote';
@Component({
  selector: 'qbs-studio-quote-calculator',
  imports: [ReckonerQuoteViewComponent],
  templateUrl: './studio-quote-calculator.html',
  styleUrl: './studio-quote-calculator.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudioQuoteCalculator {
  readonly editor = inject(QUOTE_EDITOR_SERVICE);
}
