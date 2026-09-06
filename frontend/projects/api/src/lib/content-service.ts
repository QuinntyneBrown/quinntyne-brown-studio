import { Injectable, inject } from '@angular/core';
import { Editable, MarketingContent } from '@qbs/domain/models';
import { IContentService } from './content.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class ContentService implements IContentService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<MarketingContent[]> {
    return this.transport.get<MarketingContent[]>('admin/content');
  }
  published(key: string): Promise<MarketingContent> {
    return this.transport.get<MarketingContent>('public/content/' + encodeURIComponent(key));
  }
  save(key: string, value: Editable<MarketingContent>): Promise<MarketingContent> {
    return this.transport.send<MarketingContent>(
      'PUT',
      'admin/content/' + encodeURIComponent(key),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
}
