"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";

import { navigationSections } from "@/constants/routes";
import { useAuth } from "@/hooks/use-auth";
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
  const router = useRouter();
  const { signOut, user } = useAuth();
  const [expandedItems, setExpandedItems] = useState<Record<string, boolean>>({});

  useEffect(() => {
    setExpandedItems((current) => {
      const nextState = { ...current };

      for (const section of navigationSections) {
        for (const item of section.items) {
          if (!item.children?.length) {
            continue;
          }

          const hasActiveChild = item.children.some(
            (child) => pathname === child.href || pathname.startsWith(`${child.href}/`),
          );
          const isParentActive =
            pathname === item.href || pathname.startsWith(`${item.href}/`);

          if ((hasActiveChild || isParentActive) && !nextState[item.href]) {
            nextState[item.href] = true;
          }
        }
      }

      return nextState;
    });
  }, [pathname]);

  const resolvedName = user?.displayName || user?.email || userName;
  const resolvedInitials = resolvedName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("") || userInitials;

  return (
    <>
      <div
        className={cn("sidebar-overlay", isOpen && "sidebar-overlay-open")}
        onClick={onNavigate}
      />
      <aside className={cn("sidebar", isOpen && "sidebar-open")}>
        <div className="sidebar-logo">
          <div className="sidebar-wordmark">FindBook</div>
          <div className="sidebar-tagline">Rental &amp; Delivery</div>
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
                  const isExpanded = expandedItems[item.href] ?? false;

                  return (
                    <div key={item.href} className="sidebar-group">
                      <div className={cn("sidebar-link-row", isActive && "sidebar-link-row-active")}>
                        <Link
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
                        {item.children?.length ? (
                          <button
                            aria-expanded={isExpanded}
                            aria-label={`Toggle ${item.label}`}
                            className="sidebar-expand-button"
                            onClick={() =>
                              setExpandedItems((current) => ({
                                ...current,
                                [item.href]: !isExpanded,
                              }))
                            }
                            type="button"
                          >
                            {isExpanded ? "−" : "+"}
                          </button>
                        ) : null}
                      </div>

                      {item.children?.length && isExpanded ? (
                        <div className="sidebar-subitems">
                          {item.children.map((child) => {
                            const isChildActive =
                              pathname === child.href || pathname.startsWith(`${child.href}/`);

                            return (
                              <Link
                                key={child.href}
                                className={cn(
                                  "sidebar-sublink",
                                  isChildActive && "sidebar-sublink-active",
                                )}
                                href={child.href}
                                onClick={onNavigate}
                              >
                                <span className="sidebar-sublink-mark">{child.shortLabel}</span>
                                <span>{child.label}</span>
                              </Link>
                            );
                          })}
                        </div>
                      ) : null}
                    </div>
                  );
                })}
              </div>
            </div>
          ))}
        </div>

        <div className="sidebar-footer">
          <div className="sidebar-user-avatar">{resolvedInitials}</div>
          <div className="sidebar-user-meta">
            <div className="sidebar-user-name">{resolvedName}</div>
            <div className="sidebar-user-plan">Reader Account</div>
            <button
              className="sidebar-signout"
              onClick={async () => {
                await signOut();
                router.replace("/login");
                router.refresh();
              }}
              type="button"
            >
              Sign Out
            </button>
          </div>
        </div>
      </aside>
    </>
  );
}
