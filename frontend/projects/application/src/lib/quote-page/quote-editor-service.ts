import { Injectable, OnDestroy, computed, inject, signal } from '@angular/core';
import { QUOTE_SERVICE } from '@qbs/api';
import {
  IQuoteEditorService,
  QuoteDraft,
  QuoteFailure,
  QuoteInput,
  QuoteResult,
  QuoteStudioOption,
  ResolvedLocation,
} from '@qbs/domain';
@Injectable()
export class QuoteEditorService implements IQuoteEditorService, OnDestroy {
  private readonly api = inject(QUOTE_SERVICE);
  private revision = 0;
  private lookupRevision = 0;
  private timer?: ReturnType<typeof setTimeout>;
  private destroyed = false;
  private readonly toronto = new Intl.DateTimeFormat('en-CA', {
    timeZone: 'America/Toronto',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
  });
  private local(instant: Date) {
    const p = Object.fromEntries(this.toronto.formatToParts(instant).map((x) => [x.type, x.value]));
    return { date: `${p['year']}-${p['month']}-${p['day']}`, time: `${p['hour']}:${p['minute']}` };
  }
  private tomorrow = new Date(Date.parse(this.local(new Date()).date + 'T12:00:00Z') + 86400000)
    .toISOString()
    .slice(0, 10);
  readonly draft = signal<QuoteDraft>({
    service: 'Wedding',
    date: this.tomorrow,
    endDate: this.tomorrow,
    start: '10:00',
    end: '12:00',
    startOffset: '',
    endOffset: '',
    assistantCount: 0,
    equipmentUnits: 0,
    lunchCount: 0,
    code: '',
    locations: [],
  });
  readonly result = signal<QuoteResult | null>(null);
  readonly status = signal<'incomplete' | 'invalid' | 'updating' | 'current' | 'unavailable'>(
    'incomplete',
  );
  readonly errors = signal<Record<string, string>>({});
  readonly message = signal('');
  readonly address = signal('');
  readonly candidates = signal<ResolvedLocation[]>([]);
  readonly lookupMessage = signal('');
  readonly resolving = signal(false);
  readonly studios = signal<QuoteStudioOption[]>([]);
  readonly studiosMessage = signal('');
  readonly studiosBusy = signal(false);
  readonly startAmbiguous = computed(
    () => this.offsets(this.draft().date, this.draft().start).length > 1,
  );
  readonly endAmbiguous = computed(
    () => this.offsets(this.draft().endDate, this.draft().end).length > 1,
  );
  private offsets(date: string, time: string): string[] {
    if (!/^\d{4}-\d{2}-\d{2}$/.test(date) || !/^\d{2}:\d{2}$/.test(time)) return [];
    return ['-04:00', '-05:00'].filter((offset) => {
      const instant = new Date(`${date}T${time}:00${offset}`);
      if (!Number.isFinite(instant.getTime())) return false;
      const local = this.local(instant);
      return local.date === date && local.time === time;
    });
  }
  async initialize() {
    await this.loadStudios();
  }
  async loadStudios() {
    if (this.studiosBusy()) return;
    this.studiosBusy.set(true);
    this.studiosMessage.set('');
    try {
      const studios = await this.api.getStudios();
      if (!this.destroyed) this.studios.set(studios);
    } catch (error) {
      if (!this.destroyed)
        this.studiosMessage.set(this.error(error, 'Studio options are unavailable.'));
    } finally {
      if (!this.destroyed) this.studiosBusy.set(false);
    }
  }
  set<K extends keyof QuoteDraft>(field: K, value: QuoteDraft[K]) {
    this.draft.update((prior) => {
      const next = { ...prior, [field]: value };
      if (field === 'date') {
        next.startOffset = '';
        if (prior.endDate === prior.date) {
          next.endDate = value as string;
          next.endOffset = '';
        }
      }
      if (field === 'start') next.startOffset = '';
      if (field === 'end' || field === 'endDate') next.endOffset = '';
      return next;
    });
    this.changed();
  }
  setLocation<K extends keyof QuoteInput['locations'][number]>(
    index: number,
    field: K,
    value: QuoteInput['locations'][number][K],
  ) {
    this.draft.update((prior) => ({
      ...prior,
      locations: prior.locations.map((stop, i) =>
        i === index
          ? {
              ...stop,
              [field]: value,
              ...(field === 'studioId' && !value ? { studioHours: '0' } : {}),
            }
          : stop,
      ),
    }));
    this.changed();
  }
  setAddress(value: string) {
    this.lookupRevision++;
    this.address.set(value);
    this.candidates.set([]);
    this.lookupMessage.set('');
    this.resolving.set(false);
    this.changed();
  }
  async resolve() {
    const address = this.address().trim();
    if (!address || this.resolving()) return;
    const revision = ++this.lookupRevision;
    this.candidates.set([]);
    this.lookupMessage.set('');
    this.resolving.set(true);
    try {
      const candidates = await this.api.resolveLocation(address);
      if (!this.destroyed && revision === this.lookupRevision) {
        this.candidates.set(candidates);
        this.lookupMessage.set(
          candidates.length
            ? 'Select the matching address.'
            : 'No matching addresses. Refine the address and try again.',
        );
      }
    } catch (error) {
      if (!this.destroyed && revision === this.lookupRevision)
        this.lookupMessage.set(this.error(error, 'Address lookup failed. Try again.'));
    } finally {
      if (!this.destroyed && revision === this.lookupRevision) this.resolving.set(false);
    }
  }
  add(location: ResolvedLocation) {
    if (this.draft().locations.length >= 100) {
      this.lookupMessage.set('Use at most 100 locations.');
      return;
    }
    this.lookupRevision++;
    this.address.set('');
    this.candidates.set([]);
    this.lookupMessage.set('');
    this.draft.update((prior) => ({
      ...prior,
      locations: [
        ...prior.locations,
        { location, parkingAmount: '0', studioId: null, studioHours: '0' },
      ],
    }));
    this.changed();
  }
  remove(index: number) {
    this.draft.update((prior) => ({
      ...prior,
      locations: prior.locations.filter((_, i) => i !== index),
    }));
    this.changed();
  }
  retry() {
    this.changed();
  }
  private changed() {
    const revision = ++this.revision;
    clearTimeout(this.timer);
    this.result.set(null);
    this.message.set('');
    const input = this.validate(revision);
    if (!input) return;
    this.status.set('updating');
    this.timer = setTimeout(() => void this.calculate(input), 300);
  }
  private validate(revision: number): QuoteInput | null {
    const d = this.draft(),
      errors: Record<string, string> = {};
    const instant = (date: string, time: string, selected: string, field: string) => {
      if (!date) {
        errors[field === 'start' ? 'date' : 'endDate'] = 'Enter a session date.';
        return '';
      }
      if (!time) {
        errors[field] = 'Enter a session time.';
        return '';
      }
      const offsets = this.offsets(date, time);
      if (!offsets.length) {
        errors[field] = 'This Toronto time does not exist. Choose another time.';
        return '';
      }
      if (Number(time.slice(3)) % 15 !== 0) errors[field] = 'Use times on a 15-minute boundary.';
      if (offsets.length > 1 && !offsets.includes(selected)) {
        errors[field] = 'Choose an offset for the repeated Toronto time.';
        return '';
      }
      return `${date}T${time}:00${offsets.length === 1 ? offsets[0] : selected}`;
    };
    const startsAt = instant(d.date, d.start, d.startOffset, 'start');
    const endsAt = instant(d.endDate, d.end, d.endOffset, 'end');
    if (d.date && d.date < this.local(new Date()).date)
      errors['date'] = 'Session date cannot be in the past.';
    if (startsAt && endsAt && Date.parse(endsAt) <= Date.parse(startsAt))
      errors['end'] = 'End time must be after start time.';
    for (const field of ['assistantCount', 'equipmentUnits', 'lunchCount'] as const) {
      const value = d[field];
      if (value === null || !Number.isInteger(value) || value < 0 || value > 2147483647)
        errors[field] = 'Use nonnegative whole counts, up to 2147483647.';
    }
    const decimal = (value: string) =>
      /^\d+(\.\d+)?$/.test(value) && Number(value) <= 7.922816251426433e28;
    d.locations.forEach((stop, i) => {
      if (!decimal(stop.parkingAmount))
        errors[`parking-${i}`] = 'Enter a nonnegative parking amount.';
      if (stop.studioId && (!decimal(stop.studioHours) || Number(stop.studioHours) % 0.25 !== 0))
        errors[`studio-${i}`] = 'Use nonnegative studio hours in quarter-hour increments.';
    });
    this.errors.set(errors);
    if (Object.keys(errors).length) {
      this.status.set('invalid');
      this.message.set('Correct the highlighted session details.');
      return null;
    }
    if (!d.locations.length || this.address().trim()) {
      this.status.set('incomplete');
      return null;
    }
    return {
      service: d.service,
      startsAt,
      endsAt,
      locations: structuredClone(d.locations),
      assistantCount: d.assistantCount!,
      equipmentUnits: d.equipmentUnits!,
      lunchCount: d.lunchCount!,
      code: d.code,
      photographerId: null,
      inputRevision: revision,
    };
  }
  private async calculate(input: QuoteInput) {
    try {
      const result = await this.api.calculate(input);
      if (this.destroyed || input.inputRevision !== this.revision) return;
      if (result.inputRevision !== this.revision)
        throw new Error('The estimate did not match your current details. Try again.');
      this.result.set(result);
      this.status.set('current');
    } catch (error) {
      if (this.destroyed || input.inputRevision !== this.revision) return;
      this.status.set(error instanceof QuoteFailure ? error.kind : 'unavailable');
      this.message.set(this.error(error, 'Quote unavailable. Please try again.'));
      if (error instanceof QuoteFailure && error.kind === 'invalid') {
        const names: Record<string, string> = { startsAt: 'start', endsAt: 'end' };
        this.errors.set(
          Object.fromEntries(
            Object.entries(error.fields).map(([key, messages]) => [
              names[key] || key,
              messages.join(' '),
            ]),
          ),
        );
        if (error.fields['studioId']) void this.loadStudios();
      }
    }
  }
  private error(error: unknown, fallback: string) {
    return error instanceof Error ? error.message.replace(/^Error:\s*/, '') : fallback;
  }
  ngOnDestroy() {
    this.destroyed = true;
    this.revision++;
    this.lookupRevision++;
    clearTimeout(this.timer);
  }
}
