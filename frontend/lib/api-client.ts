import type { ProblemDetails } from "./types";

/**
 * Thin typed fetch wrapper over the Core API — every request goes through here so auth headers,
 * base URL, and Problem Details error parsing live in one place. See docs/04-api-spec.md#conventions.
 */

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails | null,
  ) {
    super(problem?.detail ?? `Request failed with status ${status}`);
    this.name = "ApiError";
  }
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  signal?: AbortSignal;
}

async function request<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
      // TODO(Phase 1): attach the access token from the auth store once
      // docs/11-security.md's JWT flow is wired up on the frontend.
    },
    body: options.body ? JSON.stringify(options.body) : undefined,
    signal: options.signal,
    credentials: "include",
  });

  if (!response.ok) {
    const problem = await response
      .json()
      .catch(() => null) as ProblemDetails | null;
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
}

export const apiClient = {
  get: <TResponse>(path: string, signal?: AbortSignal) =>
    request<TResponse>(path, { method: "GET", signal }),
  post: <TResponse>(path: string, body?: unknown, signal?: AbortSignal) =>
    request<TResponse>(path, { method: "POST", body, signal }),
  put: <TResponse>(path: string, body?: unknown, signal?: AbortSignal) =>
    request<TResponse>(path, { method: "PUT", body, signal }),
  delete: <TResponse>(path: string, signal?: AbortSignal) =>
    request<TResponse>(path, { method: "DELETE", signal }),
};
