export interface Promotion {
  id: string;
  version: number;
  title: string;
  description: string;
  indicativePrice: string;
  published: boolean;
  consultationNotice?: string;
}
