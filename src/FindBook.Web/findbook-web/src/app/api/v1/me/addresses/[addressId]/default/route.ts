import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

type RouteContext = {
  params: Promise<{
    addressId: string;
  }>;
};

export async function POST(request: NextRequest, context: RouteContext) {
  const { addressId } = await context.params;

  return proxyToBackend(`/api/me/addresses/${addressId}/default`, {
    body: await request.text(),
    contentType: "application/json",
    method: "POST",
  });
}
