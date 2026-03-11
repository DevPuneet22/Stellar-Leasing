import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { listWorkflowDefinitions, type WorkflowDefinitionSummary } from "../../lib/api";

type DashboardState = {
  workflows: WorkflowDefinitionSummary[];
  errorMessage: string | null;
  isLoading: boolean;
};

export function DashboardOverview() {
  const [state, setState] = useState<DashboardState>({
    workflows: [],
    errorMessage: null,
    isLoading: true,
  });

  useEffect(() => {
    let isMounted = true;

    async function load() {
      try {
        const workflows = await listWorkflowDefinitions();
        if (!isMounted) {
          return;
        }

        setState({
          workflows,
          errorMessage: null,
          isLoading: false,
        });
      } catch (error) {
        if (!isMounted) {
          return;
        }

        setState({
          workflows: [],
          errorMessage: error instanceof Error ? error.message : "Unable to load dashboard metrics.",
          isLoading: false,
        });
      }
    }

    void load();

    return () => {
      isMounted = false;
    };
  }, []);

  const activeWorkflows = state.workflows.filter((workflow) => workflow.activeVersionNumber !== null).length;
  const draftWorkflows = state.workflows.filter((workflow) => workflow.draftVersionNumber > 0).length;
  const unpublishedWorkflows = state.workflows.filter((workflow) => workflow.activeVersionNumber === null).length;

  const stats = [
    { label: "Workflow definitions", value: String(state.workflows.length) },
    { label: "Published versions", value: String(activeWorkflows) },
    { label: "Open drafts", value: String(draftWorkflows) },
    { label: "Needs activation", value: String(unpublishedWorkflows) },
  ];

  return (
    <section className="panel-stack">
      <header className="page-header">
        <div>
          <p className="page-header__eyebrow">Operations control</p>
          <h2 className="page-header__title">Workflow command center</h2>
        </div>
        <div className="badge">
          {state.isLoading ? "Loading live API metrics..." : `${state.workflows.length} definitions loaded`}
        </div>
      </header>

      {state.errorMessage ? <div className="notice notice--error">{state.errorMessage}</div> : null}

      <div className="stats-grid">
        {stats.map((stat) => (
          <article className="stat-card" key={stat.label}>
            <span className="stat-card__label">{stat.label}</span>
            <strong className="stat-card__value">{state.isLoading ? "..." : stat.value}</strong>
          </article>
        ))}
      </div>

      <div className="editor-grid">
        <article className="panel">
          <h3>Latest workflow definitions</h3>
          <div className="data-list">
            {state.workflows.length === 0 && !state.isLoading ? (
              <p className="muted">No workflows have been created yet.</p>
            ) : null}

            {state.workflows.slice(0, 6).map((workflow) => (
              <div className="data-row" key={workflow.id}>
                <div>
                  <strong>{workflow.name}</strong>
                  <p className="muted">{workflow.code}</p>
                  <div className="workflow-card__actions">
                    <Link className="button button--secondary" to={`/workflows/${workflow.id}`}>
                      Overview
                    </Link>
                    <Link className="button" to={`/workflows/${workflow.id}/builder`}>
                      Builder
                    </Link>
                  </div>
                </div>
                <div className="data-row__meta">
                  <span>Draft {workflow.draftVersionNumber || "-"}</span>
                  <span>Active {workflow.activeVersionNumber ?? "-"}</span>
                </div>
              </div>
            ))}
          </div>
        </article>

        <article className="panel">
          <h3>Current product coverage</h3>
          <ul className="list">
            <li>Workflow definitions now use PostgreSQL-backed persistence.</li>
            <li>The frontend can create, edit, version, and activate workflow definitions.</li>
            <li>Auth, tenants, process runtime, and task inbox are still pending.</li>
          </ul>
        </article>
      </div>
    </section>
  );
}
