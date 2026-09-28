import { NextResponse } from "next/server";

export async function GET() {
  const backendUrl = process.env.BACKEND_URL;
  if (!backendUrl) {
    return NextResponse.json({ status: "error", backend: "not_configured" }, { status: 503 });
  }

  try {
    const response = await fetch(`${backendUrl.replace(/\/$/, "")}/health`, {
      cache: "no-store",
      signal: AbortSignal.timeout(8000),
    });
    if (!response.ok) throw new Error(`Backend returned ${response.status}`);
    return NextResponse.json({ status: "ok", backend: "connected" });
  } catch {
    return NextResponse.json({ status: "error", backend: "unavailable" }, { status: 503 });
  }
}
