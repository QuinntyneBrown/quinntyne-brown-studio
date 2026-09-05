import { FieldDefinition } from './field-definition';
export interface ResourceDefinition {
  key: string;
  title: string;
  singular: string;
  description: string;
  fields: FieldDefinition[];
}
