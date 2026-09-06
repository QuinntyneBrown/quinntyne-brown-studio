import { Component, inject } from '@angular/core';
import { SESSION_PAGE_SERVICE } from '@qbs/api';
@Component({
  selector: 'qbs-session-upload',
  imports: [],
  templateUrl: './session-upload.html',
  styleUrl: './session-upload.css',
})
export class SessionUpload {
  readonly state = inject(SESSION_PAGE_SERVICE);
}
