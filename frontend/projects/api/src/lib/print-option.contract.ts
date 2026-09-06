import { Editable, PrintOption } from '@qbs/domain/models';
export interface IPrintOptionService {
  list(): Promise<PrintOption[]>;
  get(id: string): Promise<PrintOption>;
  save(value: Editable<PrintOption>): Promise<PrintOption>;
  published(): Promise<PrintOption[]>;
}
