import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

export async function POST(request: NextRequest) {
  return proxyToBackend("/api/me/addresses", {
    body: await request.text(),
    contentType: "application/json",
    method: "POST",
  });
}
