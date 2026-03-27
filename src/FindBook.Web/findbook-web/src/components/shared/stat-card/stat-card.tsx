type StatCardProps = {
  accent: "gold" | "forest" | "sky" | "rust";
  icon: string;
  label: string;
  value: string;
  change?: string;
};

export function StatCard({ accent, icon, label, value, change }: StatCardProps) {
  return (
    <article className="stat-card">
      <div className={`stat-icon stat-icon-${accent}`}>{icon}</div>
      <div className="stat-value">{value}</div>
      <div className="stat-label">{label}</div>
      {change ? <div className="stat-change">{change}</div> : null}
    </article>
  );
}
