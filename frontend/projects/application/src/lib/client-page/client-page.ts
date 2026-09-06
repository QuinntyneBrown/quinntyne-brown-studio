import { PrintRequestEditor } from '@qbs/domain';
import { AlbumEditor } from '@qbs/domain';
import { Component, inject, OnInit } from '@angular/core';
import { PhotoGrid } from '@qbs/domain';
import { RouterLink } from '@angular/router';
import { Notice, EmptyState, Dialog } from '@qbs/components';
import { CLIENT_PAGE_SERVICE } from '@qbs/api';
import { ClientPageService } from './client-page-service';
@Component({
  providers: [{ provide: CLIENT_PAGE_SERVICE, useClass: ClientPageService }],
  selector: 'qbs-client-page',
  imports: [PrintRequestEditor, AlbumEditor, PhotoGrid, RouterLink, Notice, EmptyState, Dialog],
  templateUrl: './client-page.html',
  styleUrl: './client-page.css',
})
export class ClientPage implements OnInit {
  readonly state = inject(CLIENT_PAGE_SERVICE);
  ngOnInit() {
    void this.state.initialize();
  }
}
