const nodes = [
  { id: "start", title: "Start", type: "Start" },
  { id: "review", title: "Manager Review", type: "Approval" },
  { id: "ops", title: "Operations Check", type: "Task" },
  { id: "done", title: "Complete", type: "End" },
];

export function BuilderPreview() {
  return (
    <section className="panel-stack">
      <header className="page-header">
        <div>
          <p className="page-header__eyebrow">Builder preview</p>
          <h2 className="page-header__title">Schema-first editor path</h2>
        </div>
      </header>

      <article className="panel">
        <p className="panel__lead">
          This starter UI is intentionally simple. Build the workflow JSON model and CRUD editor
          first, then swap this placeholder for a React Flow canvas.
        </p>

        <div className="builder-preview">
          {nodes.map((node, index) => (
            <div className="builder-preview__node" key={node.id}>
              <span className="builder-preview__type">{node.type}</span>
              <strong>{node.title}</strong>
              {index < nodes.length - 1 ? <span className="builder-preview__arrow">→</span> : null}
            </div>
          ))}
        </div>
      </article>
    </section>
  );
}
