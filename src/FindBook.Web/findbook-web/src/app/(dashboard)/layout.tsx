import type { ReactNode } from "react";

import { DashboardLayout } from "@/components/layouts/dashboard-layout";
import { getShellData } from "@/features/book-rental/services/book-rental.service";

type DashboardRouteLayoutProps = {
  children: ReactNode;
};

export const dynamic = "force-dynamic";

export default async function DashboardRouteLayout({
  children,
}: DashboardRouteLayoutProps) {
  const shellData = await getShellData();

  return (
    <DashboardLayout
      notificationCount={shellData.notificationCount}
      notifications={shellData.notifications}
      userInitials={shellData.userInitials}
      userName={shellData.userName}
    >
      {children}
    </DashboardLayout>
  );
}
