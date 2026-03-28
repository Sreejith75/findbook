import { StatCard } from "@/components/shared/stat-card";
import { ProfileManagementPanel } from "@/features/book-rental/components/profile-management-panel";
import type { ProfileOverviewData } from "@/features/book-rental/types/book-rental.types";

type ProfileOverviewProps = {
  data: ProfileOverviewData;
};

export function ProfileOverview({ data }: ProfileOverviewProps) {
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
            {data.joinedLabel} · {data.user.addresses[0]?.city ?? "No default address"}
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
          <div className="chart-title">Account Summary</div>
          <div className="settings-list">
            <div className="settings-row">
              <div>
                <div style={{ fontWeight: 700 }}>Primary Location</div>
                <div className="subtle-text">
                  {data.user.addresses[0]
                    ? `${data.user.addresses[0].city}, ${data.user.addresses[0].state}`
                    : "No address saved yet"}
                </div>
              </div>
            </div>
            <div className="settings-row">
              <div>
                <div style={{ fontWeight: 700 }}>Phone</div>
                <div className="subtle-text">{data.user.phoneNumber ?? "No phone number saved"}</div>
              </div>
            </div>
            <div className="settings-row">
              <div>
                <div style={{ fontWeight: 700 }}>Default Address</div>
                <div className="subtle-text">
                  {data.user.addresses.find((address) => address.isDefault)
                    ? `${data.user.addresses.find((address) => address.isDefault)?.street}, ${data.user.addresses.find((address) => address.isDefault)?.city}`
                    : "Choose a default delivery address below"}
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <ProfileManagementPanel user={data.user} />
    </div>
  );
}
