import { Injectable } from '@angular/core';
import { ITorontoTimeService } from '@qbs/api';
@Injectable()
export class TorontoTimeService implements ITorontoTimeService {
  private readonly formatter = new Intl.DateTimeFormat('en-CA', {
    timeZone: 'America/Toronto',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
  });
  private formatted(date: Date) {
    const parts = Object.fromEntries(
      this.formatter.formatToParts(date).map((part) => [part.type, part.value]),
    );
    return `${parts['year']}-${parts['month']}-${parts['day']}T${parts['hour']}:${parts['minute']}`;
  }
  local(value: unknown): string {
    if (typeof value !== 'string') return '';
    if (/(Z|[+-]\d\d:\d\d)$/.test(value) && Number.isFinite(Date.parse(value)))
      return this.formatted(new Date(value));
    return value.slice(0, 16);
  }
  offsets(value: unknown): string[] {
    const local = this.local(value);
    if (!/^\d{4}-\d\d-\d\dT\d\d:\d\d$/.test(local)) return [];
    return ['-04:00', '-05:00'].filter((offset) => {
      const date = new Date(local + ':00' + offset);
      return Number.isFinite(date.valueOf()) && this.formatted(date) === local;
    });
  }
  offset(value: unknown): string {
    if (typeof value !== 'string' || !/(Z|[+-]\d\d:\d\d)$/.test(value)) return '';
    return (
      this.offsets(value).find(
        (offset) => Date.parse(this.local(value) + ':00' + offset) === Date.parse(value),
      ) ?? ''
    );
  }
  choose(value: unknown, offset: string) {
    return offset ? this.local(value) + ':00' + offset : this.local(value);
  }
  resolve(value: unknown): string {
    const local = this.local(value);
    if (!local) throw new Error('Enter a date and time.');
    if (Number(local.slice(14, 16)) % 15 !== 0) throw new Error('Use 15-minute increments.');
    const choices = this.offsets(value);
    if (!choices.length) throw new Error('This time does not exist in Toronto.');
    const offset = this.offset(value);
    if (choices.length > 1 && !choices.includes(offset))
      throw new Error('Choose an occurrence for the repeated Toronto time.');
    return local + ':00' + (choices.length === 1 ? choices[0] : offset);
  }
  problem(value: unknown): string {
    if (!value) return '';
    try {
      this.resolve(value);
      return '';
    } catch (error) {
      return error instanceof Error ? error.message : 'Check this time.';
    }
  }
}
