import { Component, input } from '@angular/core';
@Component({
  selector: 'qbs-empty',
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.css',
})
export class EmptyState {
  message = input('Nothing here yet.');
}
