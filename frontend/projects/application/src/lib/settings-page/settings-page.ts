import { SettingsEditor } from '@qbs/domain';
import { Component, inject, OnInit } from '@angular/core';
import { Notice } from '@qbs/components';
import { SETTINGS_PAGE_SERVICE } from '@qbs/api';
import { SettingsPageService } from './settings-page-service';
@Component({
  providers: [{ provide: SETTINGS_PAGE_SERVICE, useClass: SettingsPageService }],
  selector: 'qbs-settings-page',
  imports: [SettingsEditor, Notice],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.css',
})
export class SettingsPage implements OnInit {
  readonly state = inject(SETTINGS_PAGE_SERVICE);
  ngOnInit() {
    void this.state.initialize();
  }
}
