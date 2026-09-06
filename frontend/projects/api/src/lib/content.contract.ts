import { Editable, MarketingContent } from '@qbs/domain/models';
export interface IContentService {
  list(): Promise<MarketingContent[]>;
  published(key: string): Promise<MarketingContent>;
  save(key: string, value: Editable<MarketingContent>): Promise<MarketingContent>;
}
