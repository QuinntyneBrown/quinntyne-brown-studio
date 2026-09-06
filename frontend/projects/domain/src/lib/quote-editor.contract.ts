import { Signal } from '@angular/core';
import { QuoteDraft } from './quote-draft';
import { QuoteInput } from './quote-input';
import { QuoteResult } from './quote-result';
import { QuoteStudioOption } from './quote-studio-option';
import { ResolvedLocation } from './resolved-location';
export interface IQuoteEditorService {
  readonly draft: Signal<QuoteDraft>;
  readonly result: Signal<QuoteResult | null>;
  readonly status: Signal<'incomplete' | 'invalid' | 'updating' | 'current' | 'unavailable'>;
  readonly errors: Signal<Record<string, string>>;
  readonly message: Signal<string>;
  readonly address: Signal<string>;
  readonly candidates: Signal<ResolvedLocation[]>;
  readonly lookupMessage: Signal<string>;
  readonly resolving: Signal<boolean>;
  readonly studios: Signal<QuoteStudioOption[]>;
  readonly studiosMessage: Signal<string>;
  readonly studiosBusy: Signal<boolean>;
  readonly startAmbiguous: Signal<boolean>;
  readonly endAmbiguous: Signal<boolean>;
  set<K extends keyof QuoteDraft>(field: K, value: QuoteDraft[K]): void;
  setLocation<K extends keyof QuoteInput['locations'][number]>(
    index: number,
    field: K,
    value: QuoteInput['locations'][number][K],
  ): void;
  setAddress(value: string): void;
  resolve(): Promise<void>;
  add(location: ResolvedLocation): void;
  remove(index: number): void;
  retry(): void;
  loadStudios(): Promise<void>;
  initialize(): Promise<void>;
}
