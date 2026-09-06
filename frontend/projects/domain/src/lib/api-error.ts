export class ApiError extends Error {
  constructor(
    public kind:
      'invalid' | 'conflict' | 'unavailable' | 'forbidden' | 'unauthenticated' | 'not-found',
    message: string,
    public errors: Record<string, string[]> = {},
  ) {
    super(message);
  }
}
