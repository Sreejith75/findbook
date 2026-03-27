"use client";

import type { ReactNode } from "react";
import { useState } from "react";

import { ShellSidebar } from "@/components/shared/shell-sidebar";
import { ShellTopbar } from "@/components/shared/shell-topbar";
import type { NotificationItem } from "@/features/book-rental/types/book-rental.types";

type DashboardLayoutProps = {
  children: ReactNode;
  notificationCount: number;
  notifications: NotificationItem[];
  userInitials: string;
  userName: string;
};

export function DashboardLayout({
  children,
  notificationCount,
  notifications,
  userInitials,
  userName,
}: DashboardLayoutProps) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  return (
    <div className="dashboard-shell">
      <ShellSidebar
        isOpen={isSidebarOpen}
        onNavigate={() => setIsSidebarOpen(false)}
        userInitials={userInitials}
        userName={userName}
      />
      <div className="dashboard-main">
        <ShellTopbar
          notificationCount={notificationCount}
          notifications={notifications}
          onMenuClick={() => setIsSidebarOpen((current) => !current)}
        />
        <main className="dashboard-content">{children}</main>
      </div>
    </div>
  );
}
