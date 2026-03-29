import "server-only";

import { redirect } from "next/navigation";

import { authSessionSchema } from "@/features/book-rental/schemas/book-rental.schema";
import type { ApiAuthSession } from "@/features/book-rental/types/book-rental.types";
import { requestBackend } from "@/lib/backend-api";
import { getDefaultRouteForRole, isAdminRole } from "@/lib/role-routing";

export async function getServerSession(): Promise<ApiAuthSession> {
  const response = await requestBackend("/api/auth/session", { method: "GET" });
  const bodyText = response.body.toString("utf8");

  if (response.status < 200 || response.status >= 300) {
    throw new Error(`Request to /api/auth/session failed with status ${response.status}: ${bodyText}`);
  }

  return authSessionSchema.parse(JSON.parse(bodyText));
}

export async function requireAdminSession(): Promise<ApiAuthSession> {
  const session = await getServerSession();
  if (!isAdminRole(session.role)) {
    redirect(getDefaultRouteForRole(session.role));
  }

  return session;
}

export async function requireReaderSession(): Promise<ApiAuthSession> {
  const session = await getServerSession();
  if (session.role !== "User") {
    redirect(getDefaultRouteForRole(session.role));
  }

  return session;
}
