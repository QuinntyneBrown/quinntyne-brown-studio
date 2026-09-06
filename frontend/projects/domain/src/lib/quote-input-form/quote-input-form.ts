import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Notice } from '@qbs/components';
import { QUOTE_EDITOR_SERVICE } from '../quote-editor.token';
@Component({
  selector: 'qbs-quote-input-form',
  imports: [FormsModule, Notice],
  templateUrl: './quote-input-form.html',
  styleUrl: './quote-input-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuoteInputForm {
  readonly editor = inject(QUOTE_EDITOR_SERVICE);
}
