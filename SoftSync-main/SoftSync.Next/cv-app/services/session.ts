let accessToken: string | null = null;
export function setCvAccessToken(token: string | null) { accessToken = token; }
export function cvFetch(input: string, init?: RequestInit) {
  const headers = new Headers(init?.headers);
  if (accessToken && input.startsWith('/api/cv/')) headers.set('Authorization', `Bearer ${accessToken}`);
  return fetch(input, { ...init, headers });
}
