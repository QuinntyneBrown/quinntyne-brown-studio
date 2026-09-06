import { Component, inject } from '@angular/core';
import { SETTINGS_PAGE_SERVICE } from '@qbs/api';
import { FormsModule } from '@angular/forms';
import { DateTimeField } from '@qbs/components';
@Component({
  selector: 'qbs-settings-editor',
  imports: [FormsModule, DateTimeField],
  templateUrl: './settings-editor.html',
  styleUrl: './settings-editor.css',
})
export class SettingsEditor {
  readonly state = inject(SETTINGS_PAGE_SERVICE);
}
