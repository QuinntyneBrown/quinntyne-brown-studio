export interface ITorontoTimeService {
  local(value: unknown): string;
  offset(value: unknown): string;
  offsets(value: unknown): string[];
  choose(value: unknown, offset: string): string;
  resolve(value: unknown): string;
  problem(value: unknown): string;
}
