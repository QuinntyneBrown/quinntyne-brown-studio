import { Component, inject, signal, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { QUOTES_API } from '@qbs/api';
import { QuoteInput, QuoteResult, ResolvedLocation } from '@qbs/domain';
import { Notice } from '@qbs/components';
@Component({
  selector: 'qbs-quote-page',
  imports: [FormsModule, Notice],
  templateUrl: './quote-page.html',
  styleUrl: './quote-page.css',
})
export class QuotePage implements OnInit, OnDestroy {
  private api = inject(QUOTES_API);
  private timer?: ReturnType<typeof setTimeout>;
  result = signal<QuoteResult | null>(null);
  message = signal('');
  busy = signal(false);
  studios = signal<any[]>([]);
  addresses = signal<ResolvedLocation[]>([]);
  address = '';
  date = new Date(Date.now() + 86400000).toISOString().slice(0, 10);
  start = '10:00';
  end = '12:00';
  offset = '-04:00';
  input: QuoteInput = {
    service: 'Wedding',
    startsAt: '',
    endsAt: '',
    locations: [],
    assistantCount: 0,
    equipmentUnits: 0,
    lunchCount: 0,
    code: '',
    photographerId: null,
    inputRevision: 0,
  };
  async ngOnInit() {
    try {
      this.studios.set(await this.api.get<any[]>('public/studios'));
    } catch (e) {
      this.message.set(e instanceof Error ? e.message : 'Studio options are unavailable.');
    }
  }
  ngOnDestroy() {
    clearTimeout(this.timer);
    this.input.inputRevision++;
  }
  async resolve() {
    try {
      this.addresses.set(
        await this.api.send<ResolvedLocation[]>('POST', 'public/locations/resolve', {
          address: this.address,
        }),
      );
    } catch (e) {
      this.message.set(e instanceof Error ? e.message : 'Address lookup failed.');
    }
  }
  add(location: ResolvedLocation) {
    this.input.locations.push({ location, parkingAmount: '0', studioId: null, studioHours: '0' });
    this.address = '';
    this.addresses.set([]);
    this.changed();
  }
  remove(index: number) {
    this.input.locations.splice(index, 1);
    this.changed();
  }
  changed() {
    this.input.inputRevision++;
    this.result.set(null);
    this.message.set('');
    clearTimeout(this.timer);
    this.busy.set(false);
    if (!this.input.locations.length) return;
    const revision = this.input.inputRevision;
    this.timer = setTimeout(() => void this.calculate(revision), 300);
  }
  async calculate(revision: number) {
    this.busy.set(true);
    try {
      this.input.startsAt = this.date + 'T' + this.start + ':00' + this.offset;
      this.input.endsAt = this.date + 'T' + this.end + ':00' + this.offset;
      const result = await this.api.send<QuoteResult>(
        'POST',
        'public/quotes/calculate',
        structuredClone(this.input),
      );
      if (revision === this.input.inputRevision && result.inputRevision === revision)
        this.result.set(result);
    } catch (e) {
      if (revision === this.input.inputRevision)
        this.message.set(e instanceof Error ? e.message : 'Quote unavailable.');
    } finally {
      if (revision === this.input.inputRevision) this.busy.set(false);
    }
  }
}
