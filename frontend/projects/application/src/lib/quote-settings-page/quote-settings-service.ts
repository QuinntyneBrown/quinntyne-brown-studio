import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { IQuoteSettingsService, RECKONER_ADMIN_SERVICE, ReckonerAdminSession } from '@qbs/api';
@Injectable()
export class QuoteSettingsService implements IQuoteSettingsService {
  private readonly access = inject(RECKONER_ADMIN_SERVICE);
  private readonly destroy = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly data = toSignal(this.route.data, { initialValue: this.route.snapshot.data });
  readonly kind = computed(() => this.data()['kind'] as string);
  readonly title = computed(
    () =>
      ({
        rates: 'Quote rates',
        discounts: 'Discount rules',
        studios: 'Studios',
        'quote-availability': 'Quote availability',
        'quote-appearance': 'Quote appearance',
      })[this.kind()] ?? 'Quote settings',
  );
  private readonly current = signal<ReckonerAdminSession | null>(null);
  readonly session = this.current.asReadonly();
  private readonly pending = signal(false);
  readonly busy = this.pending.asReadonly();
  private readonly problem = signal('');
  readonly error = this.problem.asReadonly();
  private request: AbortController | undefined;
  private renewal: ReturnType<typeof setTimeout> | undefined;
  constructor() {
    this.destroy.onDestroy(() => {
      this.request?.abort();
      clearTimeout(this.renewal);
      this.current.set(null);
    });
    void this.refresh();
  }
  async refresh(): Promise<void> {
    if (this.destroy.destroyed || this.pending()) return;
    clearTimeout(this.renewal);
    const request = (this.request = new AbortController());
    this.pending.set(true);
    this.problem.set('');
    try {
      const session = await this.access.mint(request.signal);
      if (this.destroy.destroyed || request.signal.aborted) return;
      const remaining = Date.parse(session.expiresAt) - Date.now();
      if (!Number.isFinite(remaining) || remaining <= 0 || remaining > 65 * 60_000)
        throw new Error('Quote administration returned an invalid session. Try again.');
      this.current.set(session);
      this.renewal = setTimeout(() => void this.refresh(), Math.max(1_000, remaining - 5 * 60_000));
    } catch (error) {
      if (this.destroy.destroyed || request.signal.aborted) return;
      if (
        error &&
        typeof error === 'object' &&
        'kind' in error &&
        (error.kind === 'unauthenticated' || error.kind === 'forbidden')
      )
        this.current.set(null);
      this.problem.set(
        error instanceof Error ? error.message : 'Quote administration is unavailable. Try again.',
      );
    } finally {
      if (!this.destroy.destroyed) this.pending.set(false);
      if (this.request === request) this.request = undefined;
    }
  }
}
