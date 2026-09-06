import { UploadFile } from './upload-file';
export interface UploadBatch {
  id: string;
  sessionId: string;
  files: UploadFile[];
}
