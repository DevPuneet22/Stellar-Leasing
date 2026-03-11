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
          <li>
            Frontend runs on <code>http://localhost:5173</code>.
          </li>
          <li>
            API base URL is <code>{config.apiBaseUrl}</code>.
          </li>
          <li>
            Database runs in Docker on <code>localhost:55432</code>.
          </li>
        </ul>
      </article>

      <article className="panel">
        <h3>Frontend route layout</h3>
        <ul className="list">
          <li>
            <code>/workflows</code> for catalog and creation.
          </li>
          <li>
            <code>/workflows/{"{id}"}</code> for workflow overview, versions, and schema review.
          </li>
          <li>
            <code>/workflows/{"{id}"}/builder</code> for drag-and-drop editing.
          </li>
        </ul>
      </article>

      <article className="panel">
        <h3>Current workflow-definition APIs</h3>
        <ul className="list">
          <li>
            <code>GET /api/workflow-definitions</code>
          </li>
          <li>
            <code>GET /api/workflow-definitions/{"{id}"}</code>
          </li>
          <li>
            <code>POST /api/workflow-definitions</code>
          </li>
          <li>
            <code>PUT /api/workflow-definitions/{"{id}"}/draft</code>
          </li>
          <li>
            <code>POST /api/workflow-definitions/{"{id}"}/versions</code>
          </li>
          <li>
            <code>POST /api/workflow-definitions/{"{id}"}/activate</code>
          </li>
        </ul>
      </article>
    </section>
  );
}
