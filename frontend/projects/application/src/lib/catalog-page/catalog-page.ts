import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { STUDIO_API } from '@qbs/api';
import { StudioRecord, ResourceDefinition, ApiError, PhotoView } from '@qbs/domain';
import { Notice, EmptyState, PhotoGrid } from '@qbs/components';
import { RESOURCES } from '../resource-definitions';
@Component({
  selector: 'qbs-catalog-page',
  imports: [FormsModule, RouterLink, Notice, EmptyState, PhotoGrid],
  templateUrl: './catalog-page.html',
  styleUrl: './catalog-page.css',
})
export class CatalogPage implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(STUDIO_API);
  definition = signal<ResourceDefinition>(RESOURCES[0]);
  records = signal<StudioRecord[]>([]);
  editing = signal(false);
  loading = signal(true);
  message = signal('');
  error = signal(false);
  busy = signal(false);
  draft: Record<string, any> = {};
  people = signal<StudioRecord[]>([]);
  photos = signal<PhotoView[]>([]);
  selected = signal<string[]>([]);
  errors: Record<string, string[]> = {};
  ngOnInit() {
    this.route.data.subscribe((data) => {
      this.definition.set(RESOURCES.find((x) => x.key === data['resource'])!);
      this.editing.set(false);
      void this.load();
    });
  }
  async load() {
    this.loading.set(true);
    try {
      this.records.set(await this.api.get<StudioRecord[]>('admin/' + this.definition().key));
    } catch (e) {
      this.fail(e);
    } finally {
      this.loading.set(false);
    }
  }
  async edit(record?: StudioRecord) {
    this.draft = record
      ? structuredClone(record)
      : Object.fromEntries(
          this.definition().fields.map((f) => [
            f.key,
            f.type === 'checkbox'
              ? true
              : f.type === 'roles'
                ? []
                : f.type === 'number'
                  ? 0
                  : f.type === 'select'
                    ? (f.options?.[0] ?? null)
                    : '',
          ]),
        );
    this.selected.set((record?.['photoIds'] as string[]) ?? []);
    this.errors = {};
    this.message.set('');
    this.editing.set(true);
    if (this.definition().key === 'sessions')
      this.people.set(await this.api.get('admin/photographers'));
    if (this.definition().key === 'public-galleries') {
      const sessions = await this.api.get<StudioRecord[]>('admin/sessions');
      const pages = await Promise.all(
        sessions.map((s) =>
          this.api.get<{ photos: PhotoView[] }>('admin/sessions/' + s.id + '/photos'),
        ),
      );
      this.photos.set(pages.flatMap((p) => p.photos).filter((p) => p.state === 'Ready'));
    }
  }
  toggleRole(role: string) {
    const roles = this.draft['roles'] as string[];
    this.draft['roles'] = roles.includes(role) ? roles.filter((x) => x !== role) : [...roles, role];
  }
  select(id: string) {
    this.selected.update((x) => (x.includes(id) ? x.filter((v) => v !== id) : [...x, id]));
  }
  async save() {
    this.busy.set(true);
    this.message.set('');
    try {
      const payload: Record<string, any> = {
        ...this.draft,
        expectedVersion: this.draft['version'] ?? 0,
      };
      if (this.definition().key === 'equipment') payload['quantity'] = Number(payload['quantity']);
      if (this.definition().key === 'public-galleries') payload['photoIds'] = this.selected();
      await this.api.send(
        this.draft['id'] ? 'PUT' : 'POST',
        'admin/' + this.definition().key + (this.draft['id'] ? '/' + this.draft['id'] : ''),
        payload,
      );
      this.editing.set(false);
      await this.load();
      this.error.set(false);
      this.message.set('Saved successfully.');
    } catch (e) {
      this.fail(e);
    } finally {
      this.busy.set(false);
    }
  }
  fail(e: unknown) {
    this.error.set(true);
    this.message.set(e instanceof Error ? e.message : 'Unable to load.');
    this.errors = e instanceof ApiError ? e.errors : {};
  }
  name(record: StudioRecord) {
    return String(record['name'] ?? record['title'] ?? record.id);
  }
}
