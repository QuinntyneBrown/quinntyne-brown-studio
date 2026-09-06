import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { QUOTE_EDITOR_SERVICE, QuoteInputForm, QuoteSummary } from '@qbs/domain';
import { QuoteEditorService } from './quote-editor-service';
@Component({
  selector: 'qbs-quote-page',
  imports: [QuoteInputForm, QuoteSummary],
  providers: [{ provide: QUOTE_EDITOR_SERVICE, useClass: QuoteEditorService }],
  templateUrl: './quote-page.html',
  styleUrl: './quote-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuotePage implements OnInit {
  readonly editor = inject(QUOTE_EDITOR_SERVICE);
  ngOnInit() {
    void this.editor.initialize();
  }
}
