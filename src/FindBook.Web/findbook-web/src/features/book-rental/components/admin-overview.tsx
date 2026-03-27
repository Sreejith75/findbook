import { StatCard } from "@/components/shared/stat-card";
import { AdminManagementPanel } from "@/features/book-rental/components/admin-management-panel";
import type { AdminOverviewData } from "@/features/book-rental/types/book-rental.types";

type AdminOverviewProps = {
  data: AdminOverviewData;
};

export function AdminOverview({ data }: AdminOverviewProps) {
  const adminStats = [
    {
      accent: "gold" as const,
      icon: "BK",
      label: "Total Books",
      value: String(data.overview.totalBooks),
      change: `${data.overview.availableBooks} currently available`,
    },
    {
      accent: "forest" as const,
      icon: "US",
      label: "Active Users",
      value: String(data.overview.totalUsers),
      change: `${data.overview.totalLibraries} connected libraries`,
    },
    {
      accent: "sky" as const,
      icon: "RT",
      label: "Active Rentals",
      value: String(data.overview.activeRentals),
      change: `${data.overview.overdueRentals} overdue`,
    },
    {
      accent: "rust" as const,
      icon: "DV",
      label: "In Delivery",
      value: String(data.overview.openDeliveryTasks),
      change: `${data.overview.completedDeliveryTasks} completed`,
    },
  ];
  const maxValue = Math.max(1, ...data.monthlyRentals);

  return (
    <div>
      <section className="stats-grid">
        {adminStats.map((stat) => (
          <StatCard key={stat.label} {...stat} />
        ))}
      </section>

      <section className="dual-column">
        <div className="chart-card">
          <div className="chart-title">Monthly Rentals</div>
          <div className="bar-chart">
            {data.monthlyRentals.map((value, index) => (
              <div className="bar-column" key={data.monthlyLabels[index]}>
                <div className="subtle-text" style={{ fontWeight: 700 }}>
                  {value}
                </div>
                <div
                  className="bar-shape"
                  style={{ height: `${Math.round((value / maxValue) * 130)}px` }}
                />
                <div className="table-muted">{data.monthlyLabels[index]}</div>
              </div>
            ))}
          </div>
        </div>

        <div className="chart-card">
          <div className="chart-title">Top Libraries</div>
          <div className="rank-list">
            {data.topLibraries.map((library, index) => (
              <div className="rank-row" key={library.id}>
                <div className="rank-label">
                  <span>
                    {index + 1}. {library.name}
                  </span>
                  <span>{library.rentals} rentals</span>
                </div>
                <div className="progress-track">
                  <div
                    className="progress-fill"
                    style={{ width: `${library.percentage}%` }}
                  />
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="page-section">
        <div className="section-header">
          <h2 className="section-title">Recent Transactions</h2>
        </div>
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>User</th>
                <th>Book</th>
                <th>Library</th>
                <th>Date</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {data.transactions.map((transaction) => (
                <tr key={transaction.id}>
                  <td>{transaction.user}</td>
                  <td>{transaction.book}</td>
                  <td className="table-muted">{transaction.library}</td>
                  <td className="table-muted">{transaction.date}</td>
                  <td>
                    <span className={`status-pill status-pill-${transaction.status}`}>
                      {transaction.statusLabel}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <AdminManagementPanel data={data} />
    </div>
  );
}
