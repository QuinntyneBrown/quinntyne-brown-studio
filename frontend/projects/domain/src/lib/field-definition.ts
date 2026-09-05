export interface FieldDefinition {
  key: string;
  label: string;
  type?:
    'text' | 'email' | 'number' | 'textarea' | 'checkbox' | 'select' | 'roles' | 'datetime-local';
  required?: boolean;
  options?: string[];
  min?: number;
  step?: string;
}
