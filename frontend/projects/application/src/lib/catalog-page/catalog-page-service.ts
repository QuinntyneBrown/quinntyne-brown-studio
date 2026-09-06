import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TORONTO_TIME_SERVICE } from '@qbs/api';
import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CATALOG_SERVICE, PHOTOGRAPHER_SERVICE, SESSION_SERVICE, PHOTO_SERVICE } from '@qbs/api';
import { Photographer, StudioRecord, ResourceDefinition, ApiError, PhotoView } from '@qbs/domain';
import { RESOURCES } from '../resource-definitions';
import { ICatalogPageService } from '@qbs/api';
@Injectable()
export class CatalogPageService implements ICatalogPageService {
  readonly loadFailed = signal(false);
  readonly time = inject(TORONTO_TIME_SERVICE);
  private readonly destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private api = inject(CATALOG_SERVICE);
  private photographers = inject(PHOTOGRAPHER_SERVICE);
  private sessions = inject(SESSION_SERVICE);
  private photoService = inject(PHOTO_SERVICE);
  definition = signal<ResourceDefinition>(RESOURCES[0]);
  records = signal<StudioRecord[]>([]);
  editing = signal(false);
  loading = signal(true);
  message = signal('');
  error = signal(false);
  busy = signal(false);
  draft = signal<Record<string, unknown>>({});
  people = signal<Photographer[]>([]);
  photos = signal<PhotoView[]>([]);
  selected = signal<string[]>([]);
  errors = signal<Record<string, string[]>>({});
  initialize() {
    this.route.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((data) => {
      this.definition.set(RESOURCES.find((x) => x.key === data['resource'])!);
      this.editing.set(false);
      void this.load();
    });
  }
  async load() {
    this.loadFailed.set(false);
    this.loading.set(true);
    this.error.set(false);
    this.message.set('');
    this.loading.set(true);
    try {
      this.records.set(await this.api.list(this.definition().key));
    } catch (e) {
      this.loadFailed.set(true);
      this.fail(e);
    } finally {
      this.loading.set(false);
    }
  }
  async edit(record?: StudioRecord) {
    this.draft.set(
      record
        ? structuredClone(record)
        : Object.fromEntries(
            this.definition().fields.map((f) => [
              f.key,
              f.type === 'checkbox'
                ? f.key !== 'published'
                : f.type === 'roles'
                  ? []
                  : f.type === 'number'
                    ? 0
                    : f.type === 'select'
                      ? (f.options?.[0] ?? null)
                      : '',
            ]),
          ),
    );
    this.selected.set((record?.['photoIds'] as string[]) ?? []);
    this.errors.set({});
    this.message.set('');
    this.editing.set(true);
    try {
      if (this.definition().key === 'sessions') this.people.set(await this.photographers.list());
      if (this.definition().key === 'public-galleries') {
        const sessions = await this.sessions.list();
        const photos: PhotoView[] = [];
        for (const session of sessions) {
          let cursor: string | null = null;
          const seen = new Set<string>();
          do {
            const page = await this.photoService.list(session.id, cursor);
            photos.push(...page.photos);
            cursor = page.nextCursor;
            if (cursor && seen.has(cursor))
              throw new Error('The photo list changed. Reopen the gallery to retry.');
            if (cursor) seen.add(cursor);
          } while (cursor);
        }
        this.photos.set(photos.filter((photo) => photo.state === 'Ready'));
      }
    } catch (error) {
      this.fail(error);
    }
  }
  move(id: string, delta: number) {
    this.selected.update((ids) => {
      const index = ids.indexOf(id),
        target = index + delta;
      if (index < 0 || target < 0 || target >= ids.length) return ids;
      const ordered = [...ids];
      [ordered[index], ordered[target]] = [ordered[target], ordered[index]];
      return ordered;
    });
  }
  cover(id: string) {
    this.selected.update((ids) =>
      ids.includes(id) ? [id, ...ids.filter((value) => value !== id)] : ids,
    );
  }
  hasRole(role: string) {
    const roles = this.draft()['roles'];
    return Array.isArray(roles) && roles.includes(role);
  }
  toggleRole(role: string) {
    const value = this.draft()['roles'];
    const roles: string[] = Array.isArray(value)
      ? value.filter((item): item is string => typeof item === 'string')
      : [];
    this.draft()['roles'] = roles.includes(role)
      ? roles.filter((item) => item !== role)
      : [...roles, role];
  }
  select(id: string) {
    this.selected.update((x) => (x.includes(id) ? x.filter((v) => v !== id) : [...x, id]));
  }
  async save() {
    if (this.busy()) return;
    this.busy.set(true);
    this.message.set('');
    try {
      const payload: Record<string, unknown> = {
        ...this.draft(),
        expectedVersion: this.draft()['version'] ?? 0,
      };
      if (this.definition().key === 'sessions') {
        payload['startsAt'] = this.time.resolve(payload['startsAt']);
        payload['endsAt'] = this.time.resolve(payload['endsAt']);
      }
      if (this.definition().key === 'equipment') payload['quantity'] = Number(payload['quantity']);
      if (this.definition().key === 'public-galleries') payload['photoIds'] = this.selected();
      await this.api.save(this.definition().key, payload);
      this.editing.set(false);
      await this.load();
      if (!this.loadFailed()) {
        this.error.set(false);
        this.message.set('Saved successfully.');
      }
    } catch (e) {
      this.fail(e);
    } finally {
      this.busy.set(false);
    }
  }
  fail(e: unknown) {
    this.error.set(true);
    this.message.set(e instanceof Error ? e.message : 'Unable to load.');
    this.errors.set(e instanceof ApiError ? e.errors : {});
  }
  name(record: StudioRecord) {
    return String(record['name'] ?? record['title'] ?? record.id);
  }
}
