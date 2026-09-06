import { TimeWindow } from './time-window';
export interface PhotographerSchedule {
  id: string;
  version: number;
  photographerId: string;
  workingWindows: TimeWindow[];
  unavailableWindows: TimeWindow[];
  buffers: {
    before: number;
    after: number;
  };
}
