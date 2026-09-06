import { Injectable, inject } from '@angular/core';
import { QuoteInput, AvailabilityResult } from '@qbs/domain/models';
import { IAvailabilityService } from './availability.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class AvailabilityService implements IAvailabilityService {
  private readonly transport = inject(STUDIO_CLIENT);
  check(input: QuoteInput): Promise<AvailabilityResult> {
    return this.transport.send('POST', 'public/availability', input);
  }
}
