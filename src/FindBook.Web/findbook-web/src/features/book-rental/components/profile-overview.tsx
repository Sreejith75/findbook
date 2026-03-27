"use client";

import { useState } from "react";

import { StatCard } from "@/components/shared/stat-card";
import { ProfileManagementPanel } from "@/features/book-rental/components/profile-management-panel";
import type { ProfileOverviewData } from "@/features/book-rental/types/book-rental.types";

type ProfileOverviewProps = {
  data: ProfileOverviewData;
};

export function ProfileOverview({ data }: ProfileOverviewProps) {
  const [settings, setSettings] = useState(data.settings);
  const initials = data.user.fullName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");

  return (
    <div>
      <section className="hero-panel profile-hero">
        <div className="profile-avatar">{initials}</div>
        <div>
          <div className="hero-panel-title" style={{ fontSize: "2rem" }}>
            {data.user.fullName}
          </div>
          <div className="hero-panel-copy" style={{ marginTop: "6px" }}>
            {data.user.email}
          </div>
          <div className="hero-panel-copy" style={{ fontSize: "13px" }}>
            {data.joinedLabel} · {data.user.addresses[0]?.city ?? "No default city"}
          </div>
        </div>
        <div className="profile-badge">{data.user.role}</div>
      </section>

      <section className="stats-grid">
        {data.profileStats.map((stat) => (
          <StatCard
            key={stat.label}
            accent={stat.accent}
            change=""
            icon={stat.icon}
            label={stat.label}
            value={stat.value}
          />
        ))}
      </section>

      <section className="dual-column">
        <div className="chart-card">
          <div className="chart-title">Favourite Genres</div>
          <div className="rank-list">
            {data.favoriteGenres.map((genre) => (
              <div className="rank-row" key={genre.id}>
                <div className="rank-label">
                  <span>{genre.name}</span>
                  <span>{genre.percentage}%</span>
                </div>
                <div className="progress-track">
                  <div
                    className="progress-fill"
                    style={{ background: genre.color, width: `${genre.percentage}%` }}
                  />
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="chart-card">
          <div className="chart-title">Notification Settings</div>
          <div className="settings-list">
            {settings.map((setting) => (
              <div className="settings-row" key={setting.id}>
                <div>
                  <div style={{ fontWeight: 700 }}>{setting.label}</div>
                  <div className="subtle-text">{setting.description}</div>
                </div>
                <button
                  className={[
                    "settings-toggle",
                    setting.enabled ? "settings-toggle-on" : "",
                  ]
                    .filter(Boolean)
                    .join(" ")}
                  onClick={() =>
                    setSettings((current) =>
                      current.map((item) =>
                        item.id === setting.id
                          ? { ...item, enabled: !item.enabled }
                          : item,
                      ),
                    )
                  }
                  type="button"
                />
              </div>
            ))}
          </div>
        </div>
      </section>

      <ProfileManagementPanel user={data.user} />
    </div>
  );
}
