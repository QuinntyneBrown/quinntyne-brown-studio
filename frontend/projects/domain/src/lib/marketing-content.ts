export interface MarketingContent {
  id: string;
  version: number;
  pageKey: string;
  heading: string;
  body: string;
  publish: boolean;
  publishedHeading: string | null;
  publishedBody: string | null;
}
