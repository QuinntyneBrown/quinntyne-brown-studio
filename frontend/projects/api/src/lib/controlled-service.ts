import { serviceError } from './service-error';
/** Acceptance composition only. Every operation is supplied by a page-object fixture. */
export function controlledService<T extends object>(service: string): T {
  return new Proxy({} as T, {
    get:
      (_target, method) =>
      async (...args: unknown[]) => {
        const invoke = (
          globalThis as typeof globalThis & {
            __qbsControlled?: (
              service: string,
              method: string,
              args: unknown[],
            ) => Promise<{
              value?: unknown;
              error?: {
                message: string;
                status: number;
                errors?: Record<string, string[]>;
              };
            }>;
          }
        ).__qbsControlled;
        if (!invoke) throw new Error('The controlled studio fixture has not been installed.');
        const result = await invoke(service, String(method), args);
        if (result.error)
          throw serviceError(result.error.status, result.error.message, result.error.errors);
        return result.value;
      },
  });
}
