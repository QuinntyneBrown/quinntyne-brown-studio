import { Inquiry } from '@qbs/domain';
import { Injectable, inject, signal } from '@angular/core';
import { INQUIRY_SERVICE } from '@qbs/api';
import { IInquiryInboxService } from '@qbs/api';
@Injectable()
export class InquiryInboxService implements IInquiryInboxService {
  readonly filter = signal('');
  readonly busy = signal(false);
  readonly loading = signal(true);
  readonly loadFailed = signal(false);
  private api = inject(INQUIRY_SERVICE);
  inquiries = signal<Inquiry[]>([]);
  selected = signal<Inquiry | null>(null);
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
      this.inquiries.set(await this.api.list(this.filter() || undefined));
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
        this.message.set('Inquiry marked reviewed.');
      }
    } catch (e) {
      this.error.set(true);
      this.message.set(e instanceof Error ? e.message : 'Unable to review.');
    } finally {
      this.busy.set(false);
    }
  }
  interestLabel(interest: string) {
    return (
      ({ Headshot: 'Headshots', FamilyPortrait: 'Family portraits' } as Record<string, string>)[
        interest
      ] ?? interest
    );
  }
}
