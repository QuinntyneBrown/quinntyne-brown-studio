import type { Provider } from '@angular/core';
import { RECKONER_PUBLIC_OPTIONS } from '@qbs/api';
import type { ReckonerPublicOptions } from '@qbs/api';
export function reckonerPublicProvider(options: ReckonerPublicOptions): Provider {
  return { provide: RECKONER_PUBLIC_OPTIONS, useValue: options };
}
export type { ReckonerPublicOptions } from '@qbs/api';
