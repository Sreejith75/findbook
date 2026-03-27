import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

type RouteContext = {
  params: Promise<{
    id: string;
  }>;
};

export async function PUT(request: NextRequest, context: RouteContext) {
  const { id } = await context.params;

  return proxyToBackend(`/api/libraries/${id}`, {
    body: await request.text(),
    contentType: "application/json",
    method: "PUT",
  });
}
