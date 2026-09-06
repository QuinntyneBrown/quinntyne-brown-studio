import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { OrderedItem } from './ordered-item';
import { OrderMove } from './order-move';
@Component({
  selector: 'qbs-ordered-selection',
  templateUrl: './ordered-selection.html',
  styleUrl: './ordered-selection.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderedSelection {
  readonly items = input<OrderedItem[]>([]);
  readonly showCover = input(false);
  readonly disabled = input(false);
  readonly moved = output<OrderMove>();
  readonly featured = output<string>();
  readonly removed = output<string>();
}
