import { config } from "../lib/config";

export function DocsPage() {
  return (
    <section className="panel-stack">
      <header className="page-header">
        <div>
          <p className="page-header__eyebrow">Project docs</p>
          <h2 className="page-header__title">What to build next</h2>
        </div>
      </header>

      <article className="panel">
        <h3>Working assumptions</h3>
        <ul className="list">
          <li>Frontend runs on `http://localhost:5173`.</li>
          <li>API base URL is `{config.apiBaseUrl}`.</li>
          <li>Database runs in Docker on `localhost:5432`.</li>
        </ul>
      </article>

      <article className="panel">
        <h3>Current docs in this repo</h3>
        <ul className="list">
          <li>`docs/prd.md` defines the MVP scope.</li>
          <li>`docs/domain-model.md` lists the core entities and rules.</li>
          <li>`docs/workflow-schema.md` defines the workflow JSON shape.</li>
          <li>`docs/api-spec.md` captures the first API surface.</li>
        </ul>
      </article>
    </section>
  );
}
