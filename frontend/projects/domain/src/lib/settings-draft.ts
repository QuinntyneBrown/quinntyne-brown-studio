import { TimeWindow } from './time-window';
export interface SettingsDraft {
  id?: string;
  version: number;
  email: string;
  pageKey: string;
  heading: string;
  body: string;
  publish: boolean;
  publishedHeading: string | null;
  publishedBody: string | null;
  photographerId: string;
  workingWindows: TimeWindow[];
  unavailableWindows: TimeWindow[];
  buffers: {
    before: number;
    after: number;
  };
}
