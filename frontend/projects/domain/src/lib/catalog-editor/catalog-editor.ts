import { Component, inject } from '@angular/core';
import { CATALOG_PAGE_SERVICE } from '@qbs/api';
import { FormsModule } from '@angular/forms';
import { DateTimeField } from '@qbs/components';
import { PhotoGrid } from '../photo-grid/photo-grid';
import { PhotoOrder } from '../photo-order/photo-order';
@Component({
  selector: 'qbs-catalog-editor',
  imports: [FormsModule, DateTimeField, PhotoGrid, PhotoOrder],
  templateUrl: './catalog-editor.html',
  styleUrl: './catalog-editor.css',
})
export class CatalogEditor {
  readonly state = inject(CATALOG_PAGE_SERVICE);
}
