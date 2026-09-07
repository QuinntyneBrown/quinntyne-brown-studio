import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TORONTO_TIME_SERVICE } from '@qbs/api';
import { SettingsDraft } from '@qbs/domain/models';
import { SettingsRecord } from '@qbs/domain/models';
import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CONTENT_SERVICE, SCHEDULE_SERVICE, CLIENT_GALLERY_SERVICE } from '@qbs/api';
import { ISettingsPageService } from '@qbs/api';
@Injectable()
export class SettingsPageService implements ISettingsPageService {
  readonly loadFailed = signal(false);
  readonly time = inject(TORONTO_TIME_SERVICE);
  private content = inject(CONTENT_SERVICE);
  private schedule = inject(SCHEDULE_SERVICE);
  private clients = inject(CLIENT_GALLERY_SERVICE);
  private readonly destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  kind = signal('content');
  title = signal('Website content');
  message = signal('');
  error = signal(false);
  loading = signal(true);
  busy = signal(false);
  items = signal<SettingsRecord[]>([]);
  draft = signal<SettingsDraft>(this.emptyDraft());
  pageKey = signal('home');
  readonly windowGroups: ('workingWindows' | 'unavailableWindows')[] = [
    'workingWindows',
    'unavailableWindows',
  ];
  initialize() {
    this.route.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((d) => {
      this.kind.set(d['kind']);
      this.title.set(
        (
          {
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
    try {
      const k = this.kind();
      if (k === 'schedule') {
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
        this.items.set(await this.content.list());
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
      email: '',
      pageKey: '',
      heading: '',
      body: '',
      publish: false,
      publishedHeading: null,
      publishedBody: null,
      photographerId: '',
      workingWindows: [],
      unavailableWindows: [],
      buffers: { before: 30, after: 30 },
    };
  }
  newRecord() {
    this.draft.set(this.emptyDraft());
  }
  label(item: SettingsRecord) {
    return 'pageKey' in item ? item.pageKey : item.email;
  }
  edit(item: SettingsRecord) {
    this.draft.set({ ...this.emptyDraft(), ...structuredClone(item) });
    if ('pageKey' in item) this.pageKey.set(item.pageKey);
  }
  async save() {
    if (this.busy()) return;
    this.busy.set(true);
    try {
      const k = this.kind();
      if (k === 'content')
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
