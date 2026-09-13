import { InquiryDetails } from '@qbs/domain';
import { DatePipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Notice, EmptyState } from '@qbs/components';
import { INQUIRY_INBOX_SERVICE } from '@qbs/api';
import { InquiryInboxService } from './inquiry-inbox-service';
@Component({
  providers: [{ provide: INQUIRY_INBOX_SERVICE, useClass: InquiryInboxService }],
  selector: 'qbs-inquiry-inbox',
  imports: [InquiryDetails, Notice, EmptyState, FormsModule, DatePipe],
  templateUrl: './inquiry-inbox.html',
  styleUrl: './inquiry-inbox.css',
})
export class InquiryInbox implements OnInit {
  readonly state = inject(INQUIRY_INBOX_SERVICE);
  ngOnInit() {
    void this.state.initialize();
  }
}
