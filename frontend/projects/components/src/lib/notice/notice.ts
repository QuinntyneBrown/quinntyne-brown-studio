import { Component, input } from '@angular/core';
@Component({ selector: 'qbs-notice', templateUrl: './notice.html', styleUrl: './notice.css' })
export class Notice {
  message = input('');
  error = input(false);
}
