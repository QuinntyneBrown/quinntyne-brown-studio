import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { STUDIO_CLIENT } from '@qbs/api';
import { Notice } from '@qbs/components';
import { ResolvedLocation } from '@qbs/domain';
@Component({
  selector: 'qbs-settings-page',
  imports: [FormsModule, Notice],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.css',
})
export class SettingsPage implements OnInit {
  private api = inject(STUDIO_CLIENT);
  private route = inject(ActivatedRoute);
  kind = signal('rates');
  title = signal('Quote rates');
  message = signal('');
  error = signal(false);
  loading = signal(true);
  busy = signal(false);
  items = signal<any[]>([]);
  candidates = signal<ResolvedLocation[]>([]);
  draft: any = {};
  address = '';
  pageKey = 'home';
  services = ['Wedding', 'Event', 'Headshot', 'FamilyPortrait'];
  costs = ['travel', 'equipment', 'lunch', 'assistant'];
  weekdays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  ngOnInit() {
    this.route.data.subscribe((d) => {
      this.kind.set(d['kind']);
      this.title.set(
        (
          {
            rates: 'Quote rates',
            discounts: 'Discount rules',
            studios: 'Studios',
            content: 'Website content',
            schedule: 'Photographer availability',
            invitations: 'Client invitations',
          } as Record<string, string>
        )[d['kind']],
      );
      void this.load();
    });
  }
  async load() {
    this.loading.set(true);
    this.message.set('');
    try {
      const k = this.kind();
      if (k === 'rates') {
        this.draft = await this.api.get<any>('admin/rates');
        this.draft.serviceRates ??= {};
        this.draft.costRates ??= {};
      } else if (k === 'discounts') {
        this.draft = await this.api.get<any>('admin/discounts');
        this.draft.advanceRule ??= { enabled: false, percentage: 0, threshold: 90 };
        this.draft.weekdayRule ??= { enabled: false, percentage: 0, weekdays: [] };
        this.draft.codeRules ??= [];
      } else if (k === 'schedule') {
        this.draft = await this.api.get<any>(
          'admin/photographers/' + this.route.snapshot.paramMap.get('id') + '/schedule',
        );
        this.draft.workingWindows ??= [];
        this.draft.unavailableWindows ??= [];
        this.draft.buffers ??= { before: 30, after: 30 };
      } else if (k === 'invitations') {
        this.draft = { email: '' };
        this.items.set(await this.api.get<any[]>('admin/clients'));
      } else {
        this.items.set(await this.api.get<any[]>('admin/' + k));
        this.newRecord();
      }
    } catch (e) {
      this.fail(e);
    } finally {
      this.loading.set(false);
    }
  }
  newRecord() {
    this.draft =
      this.kind() === 'studios'
        ? { name: '', hourlyFee: 0, enabled: true, isBase: false, resolvedAddress: null }
        : { heading: '', body: '', publish: false };
    this.address = '';
  }
  edit(item: any) {
    this.draft = structuredClone(item);
    if (item.pageKey) this.pageKey = item.pageKey;
    this.address = item.resolvedAddress?.label ?? '';
  }
  async resolve() {
    try {
      this.candidates.set(
        await this.api.send('POST', 'public/locations/resolve', { address: this.address }),
      );
    } catch (e) {
      this.fail(e);
    }
  }
  choose(candidate: ResolvedLocation) {
    this.draft.resolvedAddress = candidate;
    this.address = candidate.label;
    this.candidates.set([]);
  }
  toggleDay(day: string) {
    const values = this.draft.weekdayRule.weekdays as string[];
    this.draft.weekdayRule.weekdays = values.includes(day)
      ? values.filter((x) => x !== day)
      : [...values, day];
  }
  async save() {
    this.busy.set(true);
    try {
      const k = this.kind();
      const payload = { ...this.draft, expectedVersion: this.draft.version ?? 0 };
      let path = 'admin/' + k;
      let method: 'POST' | 'PUT' = 'PUT';
      if (k === 'studios') {
        method = this.draft.id ? 'PUT' : 'POST';
        path += this.draft.id ? '/' + this.draft.id : '';
      }
      if (k === 'content') path += '/' + this.pageKey;
      if (k === 'schedule')
        path = 'admin/photographers/' + this.route.snapshot.paramMap.get('id') + '/schedule';
      if (k === 'invitations') method = 'POST';
      await this.api.send(method, path, payload);
      await this.load();
      this.error.set(false);
      this.message.set(k === 'invitations' ? 'Invitation queued.' : 'Saved successfully.');
    } catch (e) {
      this.fail(e);
    } finally {
      this.busy.set(false);
    }
  }
  fail(e: unknown) {
    this.error.set(true);
    this.message.set(e instanceof Error ? e.message : 'Unable to save.');
  }
}
