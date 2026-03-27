import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

type RouteContext = {
  params: Promise<{
    id: string;
  }>;
};

export async function POST(_request: NextRequest, context: RouteContext) {
  const { id } = await context.params;

  return proxyToBackend(`/api/delivery-tasks/${id}/fail`, {
    method: "POST",
  });
}
