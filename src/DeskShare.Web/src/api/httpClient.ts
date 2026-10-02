interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

type UnauthorizedHandler = () => void;
let unauthorizedHandler: UnauthorizedHandler = () => {};

export function setUnauthorizedHandler(handler: UnauthorizedHandler): void {
  unauthorizedHandler = handler;
}

async function request<T>(method: string, url: string, body?: unknown): Promise<T> {
  const response = await fetch(url, {
    method,
    credentials: 'same-origin',
    headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  if (response.status === 401) unauthorizedHandler();
  if (!response.ok) throw new Error(await readErrorMessage(response));
  if (response.status === 204) return undefined as T;

  return (await response.json()) as T;
}

async function readErrorMessage(response: Response): Promise<string> {
  const problem = (await response.json().catch(() => null)) as ProblemDetails | null;
  const validationMessages = Object.values(problem?.errors ?? {}).flat();

  return validationMessages.length > 0
    ? validationMessages.join(' ')
    : (problem?.detail ?? problem?.title ?? `Request failed with status ${response.status}.`);
}

export const http = {
  get: <T>(url: string) => request<T>('GET', url),
  post: <T>(url: string, body?: unknown) => request<T>('POST', url, body ?? {}),
  put: <T>(url: string, body: unknown) => request<T>('PUT', url, body),
  delete: (url: string) => request<void>('DELETE', url),
};

export function toErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Something went wrong. Please try again.';
}
