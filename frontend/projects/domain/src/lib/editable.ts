export type Editable<T> = Omit<T, 'id' | 'version'> & {
  id?: string;
  version?: number;
  expectedVersion?: number;
};
