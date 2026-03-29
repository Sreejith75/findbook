import { Button } from "@/components/ui/button";
import type { DeliveriesOverviewData } from "@/features/book-rental/types/book-rental.types";

type DeliveriesOverviewProps = {
  data: DeliveriesOverviewData;
};

export function DeliveriesOverview({ data }: DeliveriesOverviewProps) {
  const bannerTitle =
    data.viewMode === "delivery"
      ? data.activeDelivery
        ? "Assigned task in progress"
        : "No assigned tasks right now"
      : data.viewMode === "admin"
        ? data.activeDelivery
          ? "Delivery operations active"
          : "No active delivery operations"
        : data.activeDelivery
          ? "1 delivery active"
          : "No active deliveries";

  return (
    <div>
      <div className="delivery-banner">
        <strong>{bannerTitle}</strong>
        <span>
          {data.activeDelivery?.title ?? "Completed deliveries and pickups will appear here."}
        </span>
      </div>

      <div className="split-panel">
        <div>
          <div className="section-header">
            <h2 className="section-title">Delivery History</h2>
          </div>
          <div className="activity-list">
            {data.history.map((item) => (
              <div className="activity-item panel-card" key={item.id}>
                <div
                  className="activity-icon"
                  style={{
                    background:
                      item.status === "transit"
                        ? "var(--gold-pale)"
                        : item.status === "failed"
                          ? "var(--blush)"
                        : "var(--forest-light)",
                  }}
                >
                  {item.icon}
                </div>
                <div>
                  <div style={{ fontWeight: 700 }}>{item.title}</div>
                  <div className="subtle-text" style={{ marginTop: "4px" }}>
                    {item.subtitle}
                  </div>
                </div>
                <div style={{ textAlign: "right" }}>
                  <div className="subtle-text">{item.date}</div>
                  <span className={`status-pill status-pill-${item.status}`}>
                    {item.status === "transit"
                      ? "In Transit"
                      : item.status === "failed"
                        ? "Failed"
                        : "Delivered"}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div>
          <div className="section-header">
            <h2 className="section-title">Live Tracking</h2>
          </div>
          <div className="tracker-card">
            <div className="tracker-header">
              <div className="tracker-eyebrow">
                {data.activeDelivery ? `Task #${data.activeDelivery.id}` : "No live task"}
              </div>
              <div className="tracker-title">{data.activeTitle ?? "Live tracking idle"}</div>
              <div className="tracker-subtitle">
                {data.activeSubtitle ?? "Create or activate a delivery task to see tracking."}
              </div>
            </div>
            <div className="tracker-steps">
              {data.steps.map((step) => (
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

          <div className="info-card" style={{ marginTop: "16px" }}>
            <div className="section-title" style={{ fontSize: "18px", marginBottom: "14px" }}>
              Delivery Address
            </div>
            <div className="subtle-text" style={{ lineHeight: 1.8 }}>
              {data.address ? (
                <>
                  {data.address.street}
                  <br />
                  {data.address.city}, {data.address.state} {data.address.postalCode}
                  <br />
                  {data.address.country}
                </>
              ) : (
                "No active delivery address available."
              )}
            </div>
            <div className="rental-actions" style={{ marginTop: "16px" }}>
              {data.viewMode === "reader" ? <Button variant="outline">Change Address</Button> : null}
              <Button>{data.viewMode === "delivery" ? "Contact Dispatch" : "Contact Rider"}</Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
