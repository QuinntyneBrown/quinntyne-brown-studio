import { PhotographerSchedule } from '@qbs/domain/models';
export interface IScheduleService {
  get(photographerId: string): Promise<PhotographerSchedule>;
  save(photographerId: string, value: PhotographerSchedule): Promise<PhotographerSchedule>;
}
