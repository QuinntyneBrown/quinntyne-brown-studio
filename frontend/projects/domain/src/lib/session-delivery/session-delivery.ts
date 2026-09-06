import { Component, inject } from '@angular/core';
import { SESSION_PAGE_SERVICE } from '@qbs/api';
import { FormsModule } from '@angular/forms';
import { DateTimeField } from '@qbs/components';
@Component({
  selector: 'qbs-session-delivery',
  imports: [FormsModule, DateTimeField],
  templateUrl: './session-delivery.html',
  styleUrl: './session-delivery.css',
})
export class SessionDelivery {
  readonly state = inject(SESSION_PAGE_SERVICE);
}
