import Link from "next/link";

import { Button } from "@/components/ui/button";
import { ProgressBar } from "@/components/shared/progress-bar";
import { StatCard } from "@/components/shared/stat-card";
import { BookCollection } from "@/features/book-rental/components/book-collection";
import type { HomeOverviewData } from "@/features/book-rental/types/book-rental.types";

type HomeOverviewProps = {
  data: HomeOverviewData;
};

export function HomeOverview({ data }: HomeOverviewProps) {
  return (
    <div>
      <section className="hero-panel">
        <div className="hero-panel-eyebrow">Welcome back, {data.userName}</div>
        <h1 className="hero-panel-title">
          Your next great read is <em>a tap away</em>
        </h1>
        <p className="hero-panel-copy">
          Browse thousands of books from regional libraries and get them delivered
          to your doorstep within 24 hours.
        </p>
        <div className="hero-panel-actions">
          <Link href="/catalog">
            <Button>Explore Library</Button>
          </Link>
        </div>
      </section>

      <section className="stats-grid">
        {data.stats.map((stat) => (
          <StatCard key={stat.label} {...stat} />
        ))}
      </section>

      <section className="page-grid">
        <div className="page-section">
          <div className="section-header">
            <h2 className="section-title">Trending This Week</h2>
            <Link className="section-link" href="/catalog">
              See all
            </Link>
          </div>
          <BookCollection
            books={data.featuredBooks}
            filterOptions={["All", "Fiction", "Non-Fiction", "Science", "History"]}
            user={data.currentUser}
          />
        </div>

        <div className="page-section">
          <div className="section-header">
            <h2 className="section-title">Active Delivery</h2>
          </div>
          <div className="tracker-card">
            <div className="tracker-header">
              <div className="tracker-eyebrow">Currently Shipping</div>
              <div className="tracker-title">
                {data.currentRental?.title ?? "No active delivery"}
              </div>
              <div className="tracker-subtitle">
                {data.currentRental ? `Rental #${data.currentRental.id}` : "Waiting for the next dispatch"}
              </div>
            </div>
            <div className="tracker-steps">
              {data.deliverySteps.map((step) => (
                <div
                  className={[
                    "tracker-step",
                    step.status === "complete" ? "tracker-step-done" : "",
                    step.status === "current" ? "tracker-step-current" : "",
                  ]
                    .filter(Boolean)
                    .join(" ")}
                  key={step.id}
                >
                  <div className="tracker-step-dot">{step.marker}</div>
                  <div>
                    <div className="tracker-step-title">{step.label}</div>
                    <div className="tracker-step-time">{step.time}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="panel-card" style={{ marginTop: "20px" }}>
            <div className="section-title" style={{ fontSize: "18px", marginBottom: "16px" }}>
              Currently Reading
            </div>
            <div style={{ display: "flex", gap: "14px", alignItems: "center", marginBottom: "16px" }}>
              <div
                className="rental-cover"
                style={{
                  background: data.currentRental?.accentColor ?? "#2d3a6e",
                  color: "white",
                  height: "72px",
                  width: "52px",
                }}
              >
                {data.currentRental?.emoji ?? "📘"}
              </div>
              <div>
                <div style={{ fontWeight: 700 }}>
                  {data.currentRental?.title ?? "No active rental"}
                </div>
                <div className="subtle-text">
                  {data.currentRental?.author ?? "Browse the catalog to start a rental"}
                </div>
                <div className="subtle-text" style={{ marginTop: "4px" }}>
                  Due: {data.currentRental?.dueOn ?? "Not scheduled"}
                </div>
              </div>
            </div>
            <ProgressBar
              label={`Reading progress ${data.currentRental?.progress ?? 0}%`}
              value={data.currentRental?.progress ?? 0}
            />
            <div className="subtle-text" style={{ marginTop: "8px" }}>
              {data.currentRental
                ? "Delivery and due-date progress based on your live rental record."
                : "Your next active rental will appear here."}
            </div>
          </div>
        </div>
      </section>

      <section className="page-section">
        <div className="section-header">
          <h2 className="section-title">Recent Activity</h2>
        </div>
        <div className="activity-list">
          {data.recentActivities.map((activity) => (
            <div className="activity-item panel-card" key={activity.id}>
              <div
                className="activity-icon"
                style={{
                  background:
                    activity.iconTone === "gold"
                      ? "var(--gold-pale)"
                      : activity.iconTone === "forest"
                        ? "var(--forest-light)"
                        : "var(--sky-light)",
                }}
              >
                {activity.icon}
              </div>
              <div>
                <div style={{ fontWeight: 700 }}>{activity.title}</div>
                <div className="subtle-text" style={{ marginTop: "4px" }}>
                  {activity.subtitle}
                </div>
              </div>
              <div style={{ textAlign: "right" }}>
                <div className="subtle-text">{activity.meta}</div>
                <span className={`status-pill status-pill-${activity.status}`}>
                  {activity.status === "transit"
                    ? "In Transit"
                    : activity.status === "returned"
                      ? "Returned"
                      : activity.status === "active"
                        ? "Active"
                      : "Published"}
                </span>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
