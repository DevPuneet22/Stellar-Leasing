const stats = [
  { label: "Active workflows", value: "12" },
  { label: "Pending approvals", value: "38" },
  { label: "Overdue tasks", value: "4" },
  { label: "Upcoming SLAs", value: "9" },
];

export function DashboardOverview() {
  return (
    <section className="panel-stack">
      <header className="page-header">
        <div>
          <p className="page-header__eyebrow">Operations control</p>
          <h2 className="page-header__title">Workflow command center</h2>
        </div>
        <div className="badge">API ready for auth and EF Core next</div>
      </header>

      <div className="stats-grid">
        {stats.map((stat) => (
          <article className="stat-card" key={stat.label}>
            <span className="stat-card__label">{stat.label}</span>
            <strong className="stat-card__value">{stat.value}</strong>
          </article>
        ))}
      </div>

      <article className="panel">
        <h3>Immediate next steps</h3>
        <ul className="list">
          <li>Replace the in-memory workflow repository with PostgreSQL persistence.</li>
          <li>Implement authentication, tenants, and role-based access.</li>
          <li>Build workflow definition CRUD before the drag-and-drop builder.</li>
        </ul>
      </article>
    </section>
  );
}
