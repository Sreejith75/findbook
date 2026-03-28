import "server-only";

import http from "node:http";
import https from "node:https";
import { cookies } from "next/headers";

import { AUTH_COOKIE_NAME } from "@/constants/auth";
import { getEnv } from "@/constants/config";

type BackendRequestOptions = {
  body?: Buffer | string;
  contentType?: string;
  method?: "GET" | "POST" | "PUT" | "DELETE";
};

export type BackendResponse = {
  body: Buffer;
  contentType: string | null;
  status: number;
};

export async function requestBackend(
  path: string,
  options: BackendRequestOptions = {},
): Promise<BackendResponse> {
  const { NEXT_PUBLIC_FINDBOOK_API_BASE_URL } = getEnv();
  const url = new URL(path, NEXT_PUBLIC_FINDBOOK_API_BASE_URL);
  const method = options.method ?? "GET";
  const cookieStore = await cookies();
  const authToken = cookieStore.get(AUTH_COOKIE_NAME)?.value;

  return new Promise<BackendResponse>((resolve, reject) => {
    const isHttps = url.protocol === "https:";
    const transport = isHttps ? https : http;
    const request = transport.request(
      url,
      isHttps
        ? {
            method,
            headers: buildHeaders(options.contentType, authToken),
            rejectUnauthorized: !(
              process.env.NODE_ENV !== "production" && url.hostname === "localhost"
            ),
          }
        : {
            method,
            headers: buildHeaders(options.contentType, authToken),
          },
      (response) => {
        const chunks: Buffer[] = [];

        response.on("data", (chunk) => {
          chunks.push(Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk));
        });

        response.on("end", () => {
          resolve({
            body: Buffer.concat(chunks),
            contentType: response.headers["content-type"] ?? null,
            status: response.statusCode ?? 500,
          });
        });
      },
    );

    request.on("error", reject);

    if (options.body) {
      request.write(options.body);
    }

    request.end();
  });
}

function buildHeaders(contentType?: string, authToken?: string): Record<string, string> {
  return {
    ...(contentType ? { "Content-Type": contentType } : {}),
    ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
  };
}
