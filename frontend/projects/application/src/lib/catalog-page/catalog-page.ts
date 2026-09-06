import { CatalogEditor } from '@qbs/domain';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Notice, EmptyState } from '@qbs/components';
import { CATALOG_PAGE_SERVICE } from '@qbs/api';
import { CatalogPageService } from './catalog-page-service';
@Component({
  providers: [{ provide: CATALOG_PAGE_SERVICE, useClass: CatalogPageService }],
  selector: 'qbs-catalog-page',
  imports: [CatalogEditor, RouterLink, Notice, EmptyState],
  templateUrl: './catalog-page.html',
  styleUrl: './catalog-page.css',
})
export class CatalogPage implements OnInit {
  readonly state = inject(CATALOG_PAGE_SERVICE);
  ngOnInit() {
    void this.state.initialize();
  }
}
