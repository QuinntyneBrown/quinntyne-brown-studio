import { PrintRequest } from '@qbs/domain';
import { Injectable, inject, signal } from '@angular/core';
import { PRINT_REQUEST_SERVICE } from '@qbs/api';
import { IPrintInboxService } from '@qbs/api';
@Injectable()
export class PrintInboxService implements IPrintInboxService {
  readonly filter = signal('');
  readonly busy = signal(false);
  readonly loading = signal(true);
  readonly loadFailed = signal(false);
  private api = inject(PRINT_REQUEST_SERVICE);
  requests = signal<PrintRequest[]>([]);
  selected = signal<PrintRequest | null>(null);
  message = signal('');
  error = signal(false);
  initialize() {
    void this.load();
  }
  async load() {
    this.loadFailed.set(false);
    this.loading.set(true);
    this.error.set(false);
    this.message.set('');
    try {
      this.requests.set(await this.api.list(this.filter() || undefined));
    } catch (e) {
      this.loadFailed.set(true);
      this.error.set(true);
      this.message.set(e instanceof Error ? e.message : 'Unable to load.');
    } finally {
      this.loading.set(false);
    }
  }
  async review() {
    if (this.busy() || !this.selected() || this.selected()!.state === 'Reviewed') return;
    this.busy.set(true);
    try {
      this.selected.set(await this.api.review(this.selected()!.id, this.selected()!.version));
      await this.load();
      if (!this.loadFailed()) {
        this.error.set(false);
        this.message.set('Request marked reviewed.');
      }
    } catch (e) {
      this.error.set(true);
      this.message.set(e instanceof Error ? e.message : 'Unable to review.');
    } finally {
      this.busy.set(false);
    }
  }
}
