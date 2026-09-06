import { ApiError } from '@qbs/domain/models';
export function serviceError(
  status: number,
  message: string,
  errors: Record<string, string[]> = {},
) {
  return new ApiError(
    status === 409
      ? 'conflict'
      : status === 404
        ? 'not-found'
        : status === 401
          ? 'unauthenticated'
          : status === 403
            ? 'forbidden'
            : status === 400
              ? 'invalid'
              : 'unavailable',
    message,
    errors,
  );
}
