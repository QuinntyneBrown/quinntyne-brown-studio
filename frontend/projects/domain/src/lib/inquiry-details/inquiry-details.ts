import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { INQUIRY_INBOX_SERVICE } from '@qbs/api';
@Component({
  selector: 'qbs-inquiry-details',
  imports: [DatePipe],
  templateUrl: './inquiry-details.html',
  styleUrl: './inquiry-details.css',
})
export class InquiryDetails {
  readonly state = inject(INQUIRY_INBOX_SERVICE);
}
