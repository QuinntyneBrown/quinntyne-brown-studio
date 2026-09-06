import { Component, inject, signal, OnInit } from '@angular/core';
import { PRINT_REQUEST_SERVICE } from '@qbs/api';
import { Notice, EmptyState } from '@qbs/components';
@Component({
  selector: 'qbs-print-inbox',
  imports: [Notice, EmptyState],
  templateUrl: './print-inbox.html',
  styleUrl: './print-inbox.css',
})
export class PrintInbox implements OnInit {
  private api = inject(PRINT_REQUEST_SERVICE);
  requests = signal<any[]>([]);
  selected = signal<any>(null);
  message = signal('');
  error = signal(false);
  ngOnInit() {
    void this.load();
  }
  async load() {
    try {
      this.requests.set(await this.api.get('admin/print-requests'));
    } catch (e) {
      this.error.set(true);
      this.message.set(e instanceof Error ? e.message : 'Unable to load.');
    }
  }
  async review() {
    try {
      this.selected.set(
        await this.api.send('POST', 'admin/print-requests/' + this.selected().id + '/review', {
          expectedVersion: this.selected().version,
        }),
      );
      await this.load();
      this.error.set(false);
      this.message.set('Request marked reviewed.');
    } catch (e) {
      this.error.set(true);
      this.message.set(e instanceof Error ? e.message : 'Unable to review.');
    }
  }
}
