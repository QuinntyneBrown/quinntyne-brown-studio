import { Component, computed, input, output } from '@angular/core';
import { OrderedSelection, OrderMove } from '@qbs/components';
import { PhotoView } from '../photo-view';
@Component({
  selector: 'qbs-photo-order',
  imports: [OrderedSelection],
  templateUrl: './photo-order.html',
  styleUrl: './photo-order.css',
})
export class PhotoOrder {
  readonly photos = input<PhotoView[]>([]);
  readonly selected = input<string[]>([]);
  readonly showCover = input(false);
  readonly disabled = input(false);
  readonly items = computed(() =>
    this.selected().map((id) => ({
      id,
      label: this.photos().find((photo) => photo.id === id)?.name ?? 'Unavailable photo',
    })),
  );
  readonly moved = output<OrderMove>();
  readonly featured = output<string>();
  readonly removed = output<string>();
}
