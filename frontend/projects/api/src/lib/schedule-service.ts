import { Injectable, inject } from '@angular/core';
import { PhotographerSchedule } from '@qbs/domain/models';
import { IScheduleService } from './schedule.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class ScheduleService implements IScheduleService {
  private readonly transport = inject(STUDIO_CLIENT);
  get(photographerId: string): Promise<PhotographerSchedule> {
    return this.transport.get<PhotographerSchedule>(
      `admin/photographers/${encodeURIComponent(photographerId)}/schedule`,
    );
  }
  save(photographerId: string, value: PhotographerSchedule): Promise<PhotographerSchedule> {
    return this.transport.send<PhotographerSchedule>(
      'PUT',
      `admin/photographers/${encodeURIComponent(photographerId)}/schedule`,
      { ...value, expectedVersion: value.version },
    );
  }
}
