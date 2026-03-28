import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";

import { AUTH_COOKIE_NAME } from "@/constants/auth";

const protectedPrefixes = [
  "/",
  "/catalog",
  "/rentals",
  "/deliveries",
  "/libraries",
  "/profile",
  "/admin",
];

export function middleware(request: NextRequest) {
  const token = request.cookies.get(AUTH_COOKIE_NAME)?.value;
  const { pathname, search } = request.nextUrl;

  const isLoginRoute = pathname === "/login";
  const isProtectedRoute = protectedPrefixes.some((prefix) =>
    prefix === "/"
      ? pathname === "/"
      : pathname === prefix || pathname.startsWith(`${prefix}/`),
  );

  if (!token && isProtectedRoute) {
    const loginUrl = new URL("/login", request.url);
    const returnTo = `${pathname}${search}`;
    if (returnTo !== "/login") {
      loginUrl.searchParams.set("returnTo", returnTo);
    }

    return NextResponse.redirect(loginUrl);
  }

  if (token && isLoginRoute) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/", "/login", "/catalog/:path*", "/rentals/:path*", "/deliveries/:path*", "/libraries/:path*", "/profile/:path*", "/admin/:path*"],
};
