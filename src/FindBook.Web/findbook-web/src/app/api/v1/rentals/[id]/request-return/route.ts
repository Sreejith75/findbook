import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

type RouteContext = {
  params: Promise<{
    id: string;
  }>;
};

export async function POST(request: NextRequest, context: RouteContext) {
  const { id } = await context.params;

  return proxyToBackend(`/api/rentals/${id}/request-return`, {
    body: await request.text(),
    contentType: "application/json",
    method: "POST",
  });
}
