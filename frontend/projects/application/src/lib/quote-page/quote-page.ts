import { ChangeDetectionStrategy, Component } from '@angular/core';
import { QUOTE_EDITOR_SERVICE } from '@qbs/api';
import { StudioQuoteCalculator } from '@qbs/domain';
import { QuoteEditorService } from './quote-editor-service';
@Component({
  selector: 'qbs-quote-page',
  imports: [StudioQuoteCalculator],
  providers: [{ provide: QUOTE_EDITOR_SERVICE, useClass: QuoteEditorService }],
  templateUrl: './quote-page.html',
  styleUrl: './quote-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuotePage {}
