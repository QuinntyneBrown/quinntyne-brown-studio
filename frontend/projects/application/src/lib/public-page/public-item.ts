import { PhotoView } from '@qbs/domain';
export interface PublicItem {
  id: string;
  name?: string;
  title?: string;
  slug?: string;
  photos?: PhotoView[];
  dimensions?: string;
  finish?: string;
  unitPrice?: string;
  description?: string;
  indicativePrice?: string;
  consultationNotice?: string;
}
