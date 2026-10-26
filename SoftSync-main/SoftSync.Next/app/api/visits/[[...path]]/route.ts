import type { NextRequest } from "next/server";

export const runtime = "nodejs";

export async function POST(request: NextRequest, context: { params: Promise<{ path?: string[] }> }) {
  const origin = request.headers.get("origin");
  if (request.headers.get("sec-fetch-site") === "cross-site") return new Response(null, { status: 403 });
  if (origin) {
    try {
      const source = new URL(origin);
      // Standalone Next.js uses the container bind address for nextUrl.origin.
      // The HTTP Host header retains the public host used by the browser.
      if (!["http:", "https:"].includes(source.protocol) || source.host !== request.headers.get("host"))
        return new Response(null, { status: 403 });
    } catch { return new Response(null, { status: 403 }); }
  }
  const { path = [] } = await context.params;
  if (path.length > 1 || (path[0] && !/^[0-9a-f-]{36}$/i.test(path[0]))) return new Response(null, { status: 404 });
  const backend = process.env.BACKEND_URL;
  if (!backend) return new Response(null, { status: 503 });
  try {
    const response = await fetch(`${backend.replace(/\/$/, "")}/api/visits/${path[0] || ""}`, {
      method: "POST", headers: { "Content-Type": "application/json" }, body: await request.text(),
      cache: "no-store", signal: AbortSignal.timeout(8000),
    });
    return new Response(response.body, { status: response.status, headers: { "Content-Type": "application/json", "Cache-Control": "no-store" } });
  } catch { return new Response(null, { status: 503 }); }
}
