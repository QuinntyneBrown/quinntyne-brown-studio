export class QuoteFailure extends Error {
  constructor(
    readonly kind: 'invalid' | 'unavailable',
    message: string,
    readonly fields: Record<string, string[]> = {},
  ) {
    super(message);
  }
}
