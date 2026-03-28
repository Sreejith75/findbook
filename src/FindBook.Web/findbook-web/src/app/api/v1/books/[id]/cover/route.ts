import type { NextRequest } from "next/server";

import { proxyToBackend } from "@/lib/backend-proxy";

type RouteContext = {
  params: Promise<{
    id: string;
  }>;
};

export async function GET(_request: NextRequest, context: RouteContext) {
  const { id } = await context.params;

  return proxyToBackend(`/api/books/${id}/cover`, {
    method: "GET",
  });
}

export async function POST(request: NextRequest, context: RouteContext) {
  const { id } = await context.params;
  const formData = await request.formData();
  const file = formData.get("file");

  if (!(file instanceof File)) {
    return new Response(JSON.stringify({ errors: { file: ["A file is required."] } }), {
      headers: {
        "content-type": "application/json",
      },
      status: 400,
    });
  }

  const boundary = `----findbook-${crypto.randomUUID()}`;
  const fileBuffer = Buffer.from(await file.arrayBuffer());
  const preamble =
    `--${boundary}\r\n` +
    `Content-Disposition: form-data; name="file"; filename="${sanitizeFileName(file.name)}"\r\n` +
    `Content-Type: ${file.type || "application/octet-stream"}\r\n\r\n`;
  const closing = `\r\n--${boundary}--\r\n`;

  return proxyToBackend(`/api/books/${id}/cover`, {
    body: Buffer.concat([
      Buffer.from(preamble, "utf8"),
      fileBuffer,
      Buffer.from(closing, "utf8"),
    ]),
    contentType: `multipart/form-data; boundary=${boundary}`,
    method: "POST",
  });
}

function sanitizeFileName(fileName: string) {
  return fileName.replace(/["\r\n]/g, "_");
}
