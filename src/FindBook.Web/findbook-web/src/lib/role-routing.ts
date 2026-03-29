import type { ApiAuthSession } from "@/features/book-rental/types/book-rental.types";

export function getDefaultRouteForRole(role: ApiAuthSession["role"]): string {
  switch (role) {
    case "DeliveryPartner":
      return "/deliveries";
    case "Admin":
    case "SuperAdmin":
      return "/admin";
    default:
      return "/";
  }
}

export function isAdminRole(role: ApiAuthSession["role"]): boolean {
  return role === "Admin" || role === "SuperAdmin";
}
