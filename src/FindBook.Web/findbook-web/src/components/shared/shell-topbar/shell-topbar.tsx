"use client";

import { FormEvent, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

import { routeTitles } from "@/constants/routes";
import type { NotificationItem } from "@/features/book-rental/types/book-rental.types";
import { cn } from "@/utils/cn";

type ShellTopbarProps = {
  notificationCount: number;
  notifications: NotificationItem[];
  onMenuClick: () => void;
};

export function ShellTopbar({
  notificationCount,
  notifications,
  onMenuClick,
}: ShellTopbarProps) {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState(searchParams.get("q") ?? "");

  const pageTitle = routeTitles[pathname] ?? "Bibliotheca";

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmedValue = searchTerm.trim();

    if (!trimmedValue) {
      router.push("/catalog");
      return;
    }

    const nextSearchParams = new URLSearchParams();
    nextSearchParams.set("q", trimmedValue);
    router.push(`/catalog?${nextSearchParams.toString()}`);
  };

  return (
    <header className="topbar">
      <button className="mobile-menu-button" onClick={onMenuClick} type="button">
        Menu
      </button>

      <div className="topbar-title">{pageTitle}</div>

      <form className="topbar-search" onSubmit={handleSubmit}>
        <input
          aria-label="Search books or authors"
          onChange={(event) => setSearchTerm(event.target.value)}
          placeholder="Search books, authors..."
          value={searchTerm}
        />
      </form>

      <div className="topbar-actions">
        <div className="notification-anchor">
          <button
            className="topbar-icon-button"
            onClick={() => setIsNotificationsOpen((current) => !current)}
            type="button"
          >
            Alerts
            {notificationCount > 0 ? <span className="notification-dot" /> : null}
          </button>

          {isNotificationsOpen ? (
            <div className="notification-panel">
              <div className="notification-panel-header">
                <span>Notifications</span>
                <button
                  className="notification-clear-button"
                  onClick={() => setIsNotificationsOpen(false)}
                  type="button"
                >
                  Close
                </button>
              </div>
              <div className="notification-list">
                {notifications.map((notification) => (
                  <div className="notification-item" key={notification.id}>
                    <span
                      className={cn(
                        "notification-status",
                        notification.isRead && "notification-status-read",
                      )}
                    />
                    <div>
                      <div className="notification-message">{notification.message}</div>
                      <div className="notification-time">{notification.timeAgo}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : null}
        </div>

        <button className="topbar-icon-button" type="button">
          Settings
        </button>
      </div>
    </header>
  );
}
