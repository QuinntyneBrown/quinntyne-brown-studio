import { Component, inject } from '@angular/core';
import { CLIENT_PAGE_SERVICE } from '@qbs/api';
import { FormsModule } from '@angular/forms';
import { PhotoGrid } from '../photo-grid/photo-grid';
import { PhotoOrder } from '../photo-order/photo-order';
@Component({
  selector: 'qbs-album-editor',
  imports: [FormsModule, PhotoGrid, PhotoOrder],
  templateUrl: './album-editor.html',
  styleUrl: './album-editor.css',
})
export class AlbumEditor {
  readonly state = inject(CLIENT_PAGE_SERVICE);
}
