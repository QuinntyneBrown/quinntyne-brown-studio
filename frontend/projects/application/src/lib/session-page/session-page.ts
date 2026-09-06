import { SessionDelivery } from '@qbs/domain';
import { SessionPhotoReview } from '@qbs/domain';
import { SessionUpload } from '@qbs/domain';
import { Component, inject, OnInit } from '@angular/core';
import { Notice, Dialog } from '@qbs/components';
import { UploadQueueService } from '../upload/upload-queue-service';
import { UPLOAD_QUEUE_SERVICE } from '@qbs/api';
import { SESSION_PAGE_SERVICE } from '@qbs/api';
import { SessionPageService } from './session-page-service';
@Component({
  selector: 'qbs-session-page',
  providers: [
    { provide: SESSION_PAGE_SERVICE, useClass: SessionPageService },
    { provide: UPLOAD_QUEUE_SERVICE, useClass: UploadQueueService },
  ],
  imports: [SessionDelivery, SessionPhotoReview, SessionUpload, Notice, Dialog],
  templateUrl: './session-page.html',
  styleUrl: './session-page.css',
})
export class SessionPage implements OnInit {
  readonly state = inject(SESSION_PAGE_SERVICE);
  ngOnInit() {
    void this.state.initialize();
  }
}
