import { Injectable, inject } from '@angular/core';
import { Editable, PrintOption } from '@qbs/domain/models';
import { IPrintOptionService } from './print-option.contract';
import { STUDIO_CLIENT } from './studio-client.token';
@Injectable()
export class PrintOptionService implements IPrintOptionService {
  private readonly transport = inject(STUDIO_CLIENT);
  list(): Promise<PrintOption[]> {
    return this.transport.get<PrintOption[]>('admin/print-options');
  }
  get(id: string): Promise<PrintOption> {
    return this.transport.get<PrintOption>(`admin/print-options/${encodeURIComponent(id)}`);
  }
  save(value: Editable<PrintOption>): Promise<PrintOption> {
    return this.transport.send<PrintOption>(
      value.id ? 'PUT' : 'POST',
      'admin/print-options' + (value.id ? '/' + encodeURIComponent(value.id) : ''),
      { ...value, expectedVersion: value.version ?? 0 },
    );
  }
  published(): Promise<PrintOption[]> {
    return this.transport.get<PrintOption[]>('public/print-options');
  }
}
