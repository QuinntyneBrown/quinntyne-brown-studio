export interface PhotoView {
  id: string;
  name: string;
  url?: string;
  thumbnailUrl?: string;
  available?: boolean;
  state?: string;
  failure?: string;
}
