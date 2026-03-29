import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

type RouteContext = {
  params: Promise<{
    addressId: string;
  }>;
};

export async function PUT(request: NextRequest, context: RouteContext) {
  const { addressId } = await context.params;

  return proxyToBackend(`/api/me/addresses/${addressId}`, {
    body: await request.text(),
    contentType: "application/json",
    method: "PUT",
  });
}

export async function DELETE(_request: NextRequest, context: RouteContext) {
  const { addressId } = await context.params;

  return proxyToBackend(`/api/me/addresses/${addressId}`, {
    method: "DELETE",
  });
}
