export interface UploadFile {
  clientFileId: string;
  name: string;
  size: number;
  sha256: string;
  photoId: string | null;
  rejection: string | null;
  state: string;
  failure: string | null;
}
