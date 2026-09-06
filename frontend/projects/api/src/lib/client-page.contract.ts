import { WritableSignal } from '@angular/core';
import { PhotoView, Album, PrintOption, PrintSelection, PrintPreview } from '@qbs/domain/models';
export interface IClientPageService {
  readonly loading: import('@angular/core').Signal<boolean>;
  readonly loadFailed: import('@angular/core').Signal<boolean>;
  kind: import('@angular/core').WritableSignal<string>;
  heading: import('@angular/core').WritableSignal<string>;
  items: import('@angular/core').WritableSignal<
    {
      id: string;
      name: string;
    }[]
  >;
  photos: import('@angular/core').WritableSignal<PhotoView[]>;
  selected: import('@angular/core').WritableSignal<string[]>;
  opened: import('@angular/core').WritableSignal<PhotoView | null>;
  message: import('@angular/core').WritableSignal<string>;
  error: import('@angular/core').WritableSignal<boolean>;
  busy: import('@angular/core').WritableSignal<boolean>;
  editing: import('@angular/core').WritableSignal<boolean>;
  album: WritableSignal<
    Partial<Album> & {
      name: string;
    }
  >;
  printOptions: import('@angular/core').WritableSignal<PrintOption[]>;
  printLines: WritableSignal<Omit<PrintSelection, 'optionRevision'>[]>;
  notes: WritableSignal<string>;
  reviewing: import('@angular/core').WritableSignal<boolean>;
  confirmation: import('@angular/core').WritableSignal<string | null>;
  readonly preview: import('@angular/core').WritableSignal<PrintPreview | null>;
  readonly pricing: import('@angular/core').WritableSignal<
    'invalid' | 'updating' | 'current' | 'idle' | 'failed'
  >;
  ngOnDestroy(): void;
  initialize(): void;
  load(): Promise<void>;
  allPhotos(): Promise<void>;
  toggle(id: string): void;
  move(id: string, delta: number): void;
  editAlbum(isNew?: boolean): Promise<void>;
  saveAlbum(): Promise<void>;
  preparePrints(): void;
  photoName(id: string): string;
  printTotal(): string | null;
  notesChanged(): void;
  priceChanged(): void;
  refreshPrice(): Promise<void>;
  submitPrints(): Promise<void>;
  action(fn: () => Promise<unknown>, message: string): Promise<void>;
  fail(e: unknown): void;
}
