import type { ReckonerPublicOptions } from '@qbs/application';
/** Browser qualification build only; the page object supplies public credentials before bootstrap. */
export const reckonerPublicOptions: ReckonerPublicOptions = (
  globalThis as typeof globalThis & { __reckonerPublicOptions?: ReckonerPublicOptions }
).__reckonerPublicOptions ?? { apiBaseUrl: '', publishableKey: '' };
