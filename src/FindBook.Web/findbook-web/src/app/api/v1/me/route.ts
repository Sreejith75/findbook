import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

export async function PUT(request: NextRequest) {
  return proxyToBackend("/api/me", {
    body: await request.text(),
    contentType: "application/json",
    method: "PUT",
  });
}
