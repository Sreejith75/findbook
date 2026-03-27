"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { navigationSections } from "@/constants/routes";
import { cn } from "@/utils/cn";

type ShellSidebarProps = {
  isOpen: boolean;
  onNavigate: () => void;
  userInitials: string;
  userName: string;
};

export function ShellSidebar({
  isOpen,
  onNavigate,
  userInitials,
  userName,
}: ShellSidebarProps) {
  const pathname = usePathname();

  return (
    <>
      <div
        className={cn("sidebar-overlay", isOpen && "sidebar-overlay-open")}
        onClick={onNavigate}
      />
      <aside className={cn("sidebar", isOpen && "sidebar-open")}>
        <div className="sidebar-logo">
          <div className="sidebar-wordmark">Bibliotheca</div>
          <div className="sidebar-tagline">Book Rental &amp; Delivery</div>
        </div>

        <div className="sidebar-nav">
          {navigationSections.map((section) => (
            <div key={section.label} className="sidebar-section">
              <div className="sidebar-section-label">{section.label}</div>
              <div className="sidebar-items">
                {section.items.map((item) => {
                  const isActive =
                    pathname === item.href ||
                    (item.href !== "/" && pathname.startsWith(item.href));

                  return (
                    <Link
                      key={item.href}
                      className={cn("sidebar-link", isActive && "sidebar-link-active")}
                      href={item.href}
                      onClick={onNavigate}
                    >
                      <span className="sidebar-link-mark">{item.shortLabel}</span>
                      <span>{item.label}</span>
                      {item.badge ? (
                        <span className="sidebar-link-badge">{item.badge}</span>
                      ) : null}
                    </Link>
                  );
                })}
              </div>
            </div>
          ))}
        </div>

        <div className="sidebar-footer">
          <div className="sidebar-user-avatar">{userInitials}</div>
          <div>
            <div className="sidebar-user-name">{userName}</div>
            <div className="sidebar-user-plan">Active Member</div>
          </div>
        </div>
      </aside>
    </>
  );
}
