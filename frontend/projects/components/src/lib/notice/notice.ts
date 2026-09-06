import { ChangeDetectionStrategy, Component, input } from '@angular/core';
@Component({
  selector: 'qbs-notice',
  templateUrl: './notice.html',
  styleUrl: './notice.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Notice {
  message = input('');
  error = input(false);
}
