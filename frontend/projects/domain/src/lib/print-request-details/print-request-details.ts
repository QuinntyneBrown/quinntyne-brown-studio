import { Component, inject } from '@angular/core';
import { PRINT_INBOX_SERVICE } from '@qbs/api';
@Component({
  selector: 'qbs-print-request-details',
  imports: [],
  templateUrl: './print-request-details.html',
  styleUrl: './print-request-details.css',
})
export class PrintRequestDetails {
  readonly state = inject(PRINT_INBOX_SERVICE);
}
