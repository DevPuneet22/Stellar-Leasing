import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  createWorkflowDefinition,
  listWorkflowDefinitions,
  type WorkflowDefinitionSummary,
} from "../lib/api";

export function WorkflowDefinitionsPage() {
  const navigate = useNavigate();
  const [workflows, setWorkflows] = useState<WorkflowDefinitionSummary[]>([]);
  const [createName, setCreateName] = useState("");
  const [createCode, setCreateCode] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    void loadWorkflows();
  }, []);

  async function loadWorkflows() {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const nextWorkflows = await listWorkflowDefinitions();
      setWorkflows(nextWorkflows);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Unable to load workflows.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleCreateWorkflow(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      const created = await createWorkflowDefinition({
        name: createName,
        code: createCode || null,
      });

      setCreateName("");
      setCreateCode("");
      await loadWorkflows();
      navigate(`/workflows/${created.id}`);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Unable to create workflow.");
    } finally {
      setIsSubmitting(false);
    }
  }

  const activeWorkflows = workflows.filter((workflow) => workflow.activeVersionNumber !== null).length;
  const draftWorkflows = workflows.filter((workflow) => workflow.draftVersionNumber > 0).length;

  return (
    <section className="panel-stack">
      <header className="page-header">
        <div>
          <p className="page-header__eyebrow">Workflow catalog</p>
          <h2 className="page-header__title">Organized workflow workspace</h2>
          <p className="muted">Create definitions here, review them on their own page, then open the builder only when you need to edit the flow.</p>
        </div>
        <div className="badge">{workflows.length} total definitions</div>
      </header>

      {errorMessage ? <div className="notice notice--error">{errorMessage}</div> : null}

      <div className="studio-layout">
        <aside className="panel panel-stack">
          <header className="section-header">
            <div>
              <h3>Create workflow</h3>
              <p className="muted">Start a new business process definition and open its dedicated overview page.</p>
            </div>
          </header>

          <form className="form-grid" onSubmit={handleCreateWorkflow}>
            <label className="field">
              <span>Name</span>
              <input
                className="input"
                value={createName}
                onChange={(event) => setCreateName(event.target.value)}
                placeholder="Lease approval"
                required
              />
            </label>

            <label className="field">
              <span>Code</span>
              <input
                className="input"
                value={createCode}
                onChange={(event) => setCreateCode(event.target.value)}
                placeholder="LEASE-APPROVAL"
              />
            </label>

            <button className="button" disabled={isSubmitting} type="submit">
              {isSubmitting ? "Working..." : "Create workflow"}
            </button>
          </form>

          <div className="stats-grid stats-grid--compact">
            <article className="stat-card">
              <span className="stat-card__label">Definitions</span>
              <strong className="stat-card__value">{isLoading ? "..." : workflows.length}</strong>
            </article>
            <article className="stat-card">
              <span className="stat-card__label">Published</span>
              <strong className="stat-card__value">{isLoading ? "..." : activeWorkflows}</strong>
            </article>
            <article className="stat-card">
              <span className="stat-card__label">Drafts</span>
              <strong className="stat-card__value">{isLoading ? "..." : draftWorkflows}</strong>
            </article>
          </div>
        </aside>

        <div className="editor-column">
          <article className="panel panel-stack">
            <header className="section-header">
              <div>
                <h3>Workflow definitions</h3>
                <p className="muted">Each workflow now has a dedicated overview page and a dedicated builder page.</p>
              </div>
            </header>

            <div className="workflow-list" aria-live="polite">
              {isLoading ? <p className="muted">Loading workflows...</p> : null}
              {!isLoading && workflows.length === 0 ? (
                <div className="empty-state">
                  <strong>No workflows yet.</strong>
                  <p>Create one to start building your business process.</p>
                </div>
              ) : null}

              {workflows.map((workflow) => (
                <article className="workflow-card workflow-card--static" key={workflow.id}>
                  <div className="workflow-card__topline">
                    <strong>{workflow.name}</strong>
                    <span className="pill">{workflow.code}</span>
                  </div>

                  <div className="workflow-card__meta">
                    <span>Draft {workflow.draftVersionNumber || "-"}</span>
                    <span>Active {workflow.activeVersionNumber ?? "-"}</span>
                  </div>

                  <div className="workflow-card__actions">
                    <Link className="button button--secondary" to={`/workflows/${workflow.id}`}>
                      Overview
                    </Link>
                    <Link className="button" to={`/workflows/${workflow.id}/builder`}>
                      Open builder
                    </Link>
                  </div>
                </article>
              ))}
            </div>
          </article>

          <article className="panel">
            <h3>How the workflow area is now organized</h3>
            <ul className="list">
              <li>`/workflows` for catalog and creation.</li>
              <li>`/workflows/{'{'}id{'}'}` for metadata, versions, and read-only review.</li>
              <li>`/workflows/{'{'}id{'}'}/builder` for drag-and-drop editing.</li>
            </ul>
          </article>
        </div>
      </div>
    </section>
  );
}
