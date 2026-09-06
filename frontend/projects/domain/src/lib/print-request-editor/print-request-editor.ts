import { Component, inject } from '@angular/core';
import { CLIENT_PAGE_SERVICE } from '@qbs/api';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'qbs-print-request-editor',
  imports: [FormsModule],
  templateUrl: './print-request-editor.html',
  styleUrl: './print-request-editor.css',
})
export class PrintRequestEditor {
  readonly state = inject(CLIENT_PAGE_SERVICE);
}
