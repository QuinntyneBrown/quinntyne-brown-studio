import { DestroyRef, Injectable, inject } from '@angular/core';
import { QUOTE_SERVICE } from '@qbs/api';
import type { IQuoteEditorService } from '@qbs/api';
import { createQuoteEditor } from 'reckoner/angular/quote';
@Injectable()
export class QuoteEditorService implements IQuoteEditorService {
  private readonly editor = createQuoteEditor({
    transport: inject(QUOTE_SERVICE),
    destroyRef: inject(DestroyRef),
  });
  readonly state = this.editor.state;
  readonly actions = this.editor.actions;
  dispose() {
    this.editor.dispose();
  }
}
