import { ChangeDetectionStrategy, Component, input } from '@angular/core';
@Component({
  selector: 'qbs-empty',
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyState {
  message = input('Nothing here yet.');
}
