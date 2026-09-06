import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TORONTO_TIME_SERVICE } from '@qbs/api';
import { SettingsDraft } from '@qbs/domain/models';
import { SettingsRecord } from '@qbs/domain/models';
import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  RATE_SERVICE,
  DISCOUNT_SERVICE,
  STUDIO_SERVICE,
  CONTENT_SERVICE,
  SCHEDULE_SERVICE,
  CLIENT_GALLERY_SERVICE,
  QUOTE_SERVICE,
} from '@qbs/api';
import { ResolvedLocation } from '@qbs/domain';
import { ISettingsPageService } from '@qbs/api';
@Injectable()
export class SettingsPageService implements ISettingsPageService {
  readonly loadFailed = signal(false);
  readonly time = inject(TORONTO_TIME_SERVICE);
  private rates = inject(RATE_SERVICE);
  private discounts = inject(DISCOUNT_SERVICE);
  private studios = inject(STUDIO_SERVICE);
  private content = inject(CONTENT_SERVICE);
  private schedule = inject(SCHEDULE_SERVICE);
  private clients = inject(CLIENT_GALLERY_SERVICE);
  private quote = inject(QUOTE_SERVICE);
  private readonly destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  kind = signal('rates');
  title = signal('Quote rates');
  message = signal('');
  error = signal(false);
  loading = signal(true);
  busy = signal(false);
  items = signal<SettingsRecord[]>([]);
  candidates = signal<ResolvedLocation[]>([]);
  private addressRevision = 0;
  draft = signal<SettingsDraft>(this.emptyDraft());
  address = signal('');
  pageKey = signal('home');
  services = ['Wedding', 'Event', 'Headshot', 'FamilyPortrait'];
  costs = ['travel', 'equipment', 'lunch', 'assistant'];
  readonly windowGroups: ('workingWindows' | 'unavailableWindows')[] = [
    'workingWindows',
    'unavailableWindows',
  ];
  weekdays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  initialize() {
    this.route.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((d) => {
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
    this.loadFailed.set(false);
    this.loading.set(true);
    this.error.set(false);
    this.message.set('');
    this.loading.set(true);
    this.message.set('');
    try {
      const k = this.kind();
      if (k === 'rates') {
        this.draft.set({ ...this.emptyDraft(), ...(await this.rates.get()) });
        this.draft().serviceRates ??= {};
        this.draft().costRates ??= {};
      } else if (k === 'discounts') {
        this.draft.set({ ...this.emptyDraft(), ...(await this.discounts.get()) });
        this.draft().advanceRule ??= { enabled: false, percentage: 0, threshold: 90, weekdays: [] };
        this.draft().weekdayRule ??= { enabled: false, percentage: 0, weekdays: [] };
        this.draft().codeRules ??= [];
      } else if (k === 'schedule') {
        this.draft.set({
          ...this.emptyDraft(),
          ...(await this.schedule.get(this.route.snapshot.paramMap.get('id')!)),
        });
        this.draft().workingWindows ??= [];
        this.draft().unavailableWindows ??= [];
        this.draft().buffers ??= { before: 30, after: 30 };
      } else if (k === 'invitations') {
        this.draft.set(this.emptyDraft());
        this.items.set(await this.clients.clients());
      } else {
        this.items.set(await (k === 'studios' ? this.studios.list() : this.content.list()));
        this.newRecord();
      }
    } catch (e) {
      this.loadFailed.set(true);
      this.fail(e);
    } finally {
      this.loading.set(false);
    }
  }
  private emptyDraft(): SettingsDraft {
    return {
      version: 0,
      name: '',
      email: '',
      hourlyFee: 0,
      enabled: true,
      isBase: false,
      resolvedAddress: null,
      pageKey: '',
      heading: '',
      body: '',
      publish: false,
      publishedHeading: null,
      publishedBody: null,
      serviceRates: {},
      costRates: {},
      advanceRule: { enabled: false, percentage: 0, threshold: 90, weekdays: [] },
      weekdayRule: { enabled: false, percentage: 0, weekdays: [] },
      codeRules: [],
      photographerId: '',
      workingWindows: [],
      unavailableWindows: [],
      buffers: { before: 30, after: 30 },
    };
  }
  newRecord() {
    this.draft.set(this.emptyDraft());
    this.addressChanged('');
  }
  addressChanged(value: string) {
    this.addressRevision++;
    this.address.set(value);
    this.candidates.set([]);
    this.draft.update((draft) => ({ ...draft, resolvedAddress: null }));
  }
  label(item: SettingsRecord) {
    return 'name' in item ? item.name : 'pageKey' in item ? item.pageKey : item.email;
  }
  edit(item: SettingsRecord) {
    this.addressRevision++;
    this.candidates.set([]);
    this.draft.set({ ...this.emptyDraft(), ...structuredClone(item) });
    if ('pageKey' in item) this.pageKey.set(item.pageKey);
    this.address.set(this.draft().resolvedAddress?.label ?? '');
  }
  async resolve() {
    const revision = ++this.addressRevision;
    this.candidates.set([]);
    try {
      const candidates = await this.quote.resolveLocation(this.address());
      if (!this.destroyRef.destroyed && revision === this.addressRevision)
        this.candidates.set(candidates);
    } catch (e) {
      if (!this.destroyRef.destroyed && revision === this.addressRevision) this.fail(e);
    }
  }
  choose(candidate: ResolvedLocation) {
    if (!this.candidates().includes(candidate)) return;
    this.addressRevision++;
    this.draft().resolvedAddress = candidate;
    this.address.set(candidate.label);
    this.candidates.set([]);
  }
  toggleDay(day: string) {
    const values = this.draft().weekdayRule.weekdays as string[];
    this.draft().weekdayRule.weekdays = values.includes(day)
      ? values.filter((x) => x !== day)
      : [...values, day];
  }
  async save() {
    if (this.busy()) return;
    this.busy.set(true);
    try {
      const k = this.kind();
      if (k === 'rates')
        await this.rates.save({
          id: this.draft().id ?? '',
          version: this.draft().version,
          serviceRates: this.draft().serviceRates,
          costRates: this.draft().costRates,
        });
      else if (k === 'discounts')
        await this.discounts.save({
          id: this.draft().id ?? '',
          version: this.draft().version,
          advanceRule: this.draft().advanceRule,
          weekdayRule: this.draft().weekdayRule,
          codeRules: this.draft().codeRules,
        });
      else if (k === 'studios')
        await this.studios.save({
          id: this.draft().id,
          version: this.draft().version,
          name: this.draft().name,
          hourlyFee: this.draft().hourlyFee,
          enabled: this.draft().enabled,
          isBase: this.draft().isBase,
          resolvedAddress: this.draft().resolvedAddress,
        });
      else if (k === 'content')
        await this.content.save(this.pageKey(), {
          id: this.draft().id,
          version: this.draft().version,
          pageKey: this.pageKey(),
          heading: this.draft().heading,
          body: this.draft().body,
          publish: this.draft().publish,
          publishedHeading: this.draft().publishedHeading,
          publishedBody: this.draft().publishedBody,
        });
      else if (k === 'schedule')
        await this.schedule.save(this.route.snapshot.paramMap.get('id')!, {
          id: this.draft().id ?? '',
          version: this.draft().version,
          photographerId: this.route.snapshot.paramMap.get('id')!,
          workingWindows: this.draft().workingWindows.map((window) => ({
            startsAt: this.time.resolve(window.startsAt),
            endsAt: this.time.resolve(window.endsAt),
          })),
          unavailableWindows: this.draft().unavailableWindows.map((window) => ({
            startsAt: this.time.resolve(window.startsAt),
            endsAt: this.time.resolve(window.endsAt),
          })),
          buffers: this.draft().buffers,
        });
      else await this.clients.invite(this.draft().email);
      await this.load();
      if (!this.loadFailed()) {
        this.error.set(false);
        this.message.set(k === 'invitations' ? 'Invitation queued.' : 'Saved successfully.');
      }
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
