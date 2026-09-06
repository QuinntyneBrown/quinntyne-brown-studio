import { Component, inject, OnInit } from '@angular/core';
import { PhotoGrid } from '@qbs/domain';
import { RouterLink } from '@angular/router';
import { EmptyState, Notice, Dialog } from '@qbs/components';
import { PUBLIC_PAGE_SERVICE } from './public-page.token';
import { PublicPageService } from './public-page-service';
@Component({
  providers: [{ provide: PUBLIC_PAGE_SERVICE, useClass: PublicPageService }],
  selector: 'qbs-public-page',
  imports: [PhotoGrid, RouterLink, EmptyState, Notice, Dialog],
  templateUrl: './public-page.html',
  styleUrl: './public-page.css',
})
export class PublicPage implements OnInit {
  readonly state = inject(PUBLIC_PAGE_SERVICE);
  ngOnInit() {
    void this.state.initialize();
  }
}
