import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

type RouteContext = {
  params: Promise<{
    addressId: string;
    id: string;
  }>;
};

export async function POST(_request: NextRequest, context: RouteContext) {
  const { addressId, id } = await context.params;

  return proxyToBackend(`/api/users/${id}/addresses/${addressId}/default`, {
    method: "POST",
  });
}
