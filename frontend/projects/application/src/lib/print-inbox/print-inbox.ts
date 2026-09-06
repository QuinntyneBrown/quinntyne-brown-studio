import { PrintRequestDetails } from '@qbs/domain';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Notice, EmptyState } from '@qbs/components';
import { PRINT_INBOX_SERVICE } from '@qbs/api';
import { PrintInboxService } from './print-inbox-service';
@Component({
  providers: [{ provide: PRINT_INBOX_SERVICE, useClass: PrintInboxService }],
  selector: 'qbs-print-inbox',
  imports: [PrintRequestDetails, Notice, EmptyState, FormsModule],
  templateUrl: './print-inbox.html',
  styleUrl: './print-inbox.css',
})
export class PrintInbox implements OnInit {
  readonly state = inject(PRINT_INBOX_SERVICE);
  ngOnInit() {
    void this.state.initialize();
  }
}
