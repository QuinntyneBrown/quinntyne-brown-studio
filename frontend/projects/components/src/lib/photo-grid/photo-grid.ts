import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { PhotoView } from '@qbs/domain';
@Component({
  selector: 'qbs-photos',
  templateUrl: './photo-grid.html',
  styleUrl: './photo-grid.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PhotoGrid {
  photos = input<PhotoView[]>([]);
  selected = input<string[]>([]);
  selectable = input(false);
  selection = output<string>();
  opened = output<PhotoView>();
  failed = signal<string[]>([]);
  open(photo: PhotoView, event: Event) {
    (event.currentTarget as HTMLElement).focus();
    this.opened.emit(photo);
  }
  fail(id: string) {
    this.failed.update((ids) => [...ids, id]);
  }
}
