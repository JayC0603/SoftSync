import type { NextRequest } from "next/server";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

const FORWARDED_REQUEST_HEADERS = ["accept", "authorization", "content-type"];
const FORWARDED_RESPONSE_HEADERS = ["content-type", "content-disposition"];

async function proxy(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  const apiBaseUrl = process.env.CV_API_URL?.replace(/\/$/, "");
  if (!apiBaseUrl) {
    return Response.json({ detail: "CV_API_URL is not configured" }, { status: 503 });
  }

  const { path } = await context.params;
  const target = new URL(`${apiBaseUrl}/${path.map(encodeURIComponent).join("/")}`);
  target.search = request.nextUrl.search;

  const headers = new Headers();
  for (const name of FORWARDED_REQUEST_HEADERS) {
    const value = request.headers.get(name);
    if (value) headers.set(name, value);
  }

  const hasBody = request.method !== "GET" && request.method !== "HEAD";
  const upstream = await fetch(target, {
    method: request.method,
    headers,
    body: hasBody ? await request.arrayBuffer() : undefined,
    cache: "no-store",
  });

  const responseHeaders = new Headers();
  for (const name of FORWARDED_RESPONSE_HEADERS) {
    const value = upstream.headers.get(name);
    if (value) responseHeaders.set(name, value);
  }

  return new Response(upstream.body, {
    status: upstream.status,
    headers: responseHeaders,
  });
}

export const GET = proxy;
export const POST = proxy;
export const PUT = proxy;
export const PATCH = proxy;
export const DELETE = proxy;
