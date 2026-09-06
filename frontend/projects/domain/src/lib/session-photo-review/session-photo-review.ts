import { Component, inject } from '@angular/core';
import { SESSION_PAGE_SERVICE } from '@qbs/api';
import { EmptyState } from '@qbs/components';
import { PhotoGrid } from '../photo-grid/photo-grid';
@Component({
  selector: 'qbs-session-photo-review',
  imports: [EmptyState, PhotoGrid],
  templateUrl: './session-photo-review.html',
  styleUrl: './session-photo-review.css',
})
export class SessionPhotoReview {
  readonly state = inject(SESSION_PAGE_SERVICE);
}
