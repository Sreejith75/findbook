import "server-only";

import { NextResponse } from "next/server";

import { requestBackend } from "@/lib/backend-api";

type ProxyOptions = {
  body?: string;
  contentType?: string;
  method?: "GET" | "POST" | "PUT" | "DELETE";
};

export async function proxyToBackend(path: string, options: ProxyOptions = {}) {
  const response = await requestBackend(path, options);

  return new NextResponse(response.body, {
    headers: response.contentType
      ? {
          "content-type": response.contentType,
        }
      : undefined,
    status: response.status,
  });
}
