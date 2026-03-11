import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  activateWorkflowVersion,
  createNextWorkflowVersion,
  getWorkflowDefinition,
  type WorkflowDefinitionDetail,
} from "../lib/api";

function buildSchemaPreview(definition: WorkflowDefinitionDetail | null) {
  if (!definition) {
    return null;
  }

  const version = definition.draftVersion ?? definition.activeVersion;
  if (!version) {
    return null;
  }

  return {
    name: definition.name,
    code: definition.code,
    version: version.versionNumber,
    trigger: {
      type: "manual",
    },
    steps: version.steps
      .slice()
      .sort((left, right) => left.sortOrder - right.sortOrder)
      .map((step) => ({
        key: step.key,
        name: step.name,
        type: step.type,
        position: {
          x: Math.round(step.positionX),
          y: Math.round(step.positionY),
        },
        ...(step.assigneeRule ? { assigneeRule: step.assigneeRule } : {}),
      })),
    transitions: version.transitions
      .slice()
      .sort((left, right) => left.sortOrder - right.sortOrder)
      .map((transition) => ({
        from: transition.from,
        to: transition.to,
        ...(transition.condition ? { condition: transition.condition } : {}),
      })),
  };
}

export function WorkflowDetailPage() {
  const { workflowId } = useParams<{ workflowId: string }>();
  const [definition, setDefinition] = useState<WorkflowDefinitionDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!workflowId) {
      return;
    }

    void loadWorkflow(workflowId);
  }, [workflowId]);

  async function loadWorkflow(id: string) {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const nextDefinition = await getWorkflowDefinition(id);
      setDefinition(nextDefinition);
    } catch (error) {
      setDefinition(null);
      setErrorMessage(error instanceof Error ? error.message : "Unable to load workflow detail.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleCreateDraftVersion() {
    if (!definition) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      const updated = await createNextWorkflowVersion(definition.id, definition.revision);
      setDefinition(updated);
      setSuccessMessage(`Created draft version ${updated.draftVersion?.versionNumber ?? ""}.`);
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Unable to create a new draft version.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleActivateDraft() {
    if (!definition?.draftVersion) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      const updated = await activateWorkflowVersion(
        definition.id,
        definition.draftVersion.versionNumber,
        definition.revision,
      );
      setDefinition(updated);
      setSuccessMessage(`Activated version ${definition.draftVersion.versionNumber}.`);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Unable to activate draft.");
    } finally {
      setIsSubmitting(false);
    }
  }

  const currentVersion = definition?.draftVersion ?? definition?.activeVersion ?? null;
  const schemaPreview = buildSchemaPreview(definition);

  return (
    <section className="panel-stack">
      <header className="page-header">
        <div>
          <p className="page-header__eyebrow">Workflow overview</p>
          <h2 className="page-header__title">
            {definition?.name ?? (isLoading ? "Loading workflow..." : "Workflow not found")}
          </h2>
          <p className="muted">Review versions, inspect the current schema, and decide when to open the builder.</p>
        </div>
        <div className="button-row">
          <Link className="button button--secondary" to="/workflows">
            Catalog
          </Link>
          {definition ? (
            <Link className="button" to={`/workflows/${definition.id}/builder`}>
              Open builder
            </Link>
          ) : null}
        </div>
      </header>

      {errorMessage ? <div className="notice notice--error">{errorMessage}</div> : null}
      {successMessage ? <div className="notice notice--success">{successMessage}</div> : null}

      {!definition ? (
        <article className="panel empty-state">
          <strong>{isLoading ? "Loading workflow..." : "Workflow unavailable"}</strong>
          <p>
            {isLoading
              ? "The workflow detail is loading."
              : "The requested workflow could not be loaded. Return to the catalog and pick another one."}
          </p>
        </article>
      ) : (
        <>
          <div className="stats-grid">
            <article className="stat-card">
              <span className="stat-card__label">Draft version</span>
              <strong className="stat-card__value">{definition.draftVersion?.versionNumber ?? "-"}</strong>
            </article>
            <article className="stat-card">
              <span className="stat-card__label">Active version</span>
              <strong className="stat-card__value">{definition.activeVersion?.versionNumber ?? "-"}</strong>
            </article>
            <article className="stat-card">
              <span className="stat-card__label">Current steps</span>
              <strong className="stat-card__value">{currentVersion?.steps.length ?? 0}</strong>
            </article>
            <article className="stat-card">
              <span className="stat-card__label">Current transitions</span>
              <strong className="stat-card__value">{currentVersion?.transitions.length ?? 0}</strong>
            </article>
          </div>

          <article className="panel panel-stack">
            <header className="section-header">
              <div>
                <h3>{definition.name}</h3>
                <p className="muted">
                  {definition.code} | {definition.draftVersion ? "Draft available for editing" : "Read-only active definition"}
                </p>
              </div>

              <div className="button-row">
                <button
                  className="button button--secondary"
                  disabled={isSubmitting || Boolean(definition.draftVersion)}
                  onClick={() => void handleCreateDraftVersion()}
                  type="button"
                >
                  New draft version
                </button>
                <button
                  className="button button--accent"
                  disabled={isSubmitting || !definition.draftVersion}
                  onClick={() => void handleActivateDraft()}
                  type="button"
                >
                  Activate draft
                </button>
              </div>
            </header>

            <div className="version-list">
              {definition.versions.map((version) => (
                <div className="version-pill" key={version.versionNumber}>
                  <strong>v{version.versionNumber}</strong>
                  <span>{version.status}</span>
                  <span>{version.stepCount} steps</span>
                  <span>{version.transitionCount} transitions</span>
                </div>
              ))}
            </div>
          </article>

          <div className="detail-grid">
            <div className="panel-stack">
              <article className="panel panel-stack">
                <header className="section-header">
                  <div>
                    <h3>Current steps</h3>
                    <p className="muted">Read-only view of the version currently shown in the product.</p>
                  </div>
                </header>

                <div className="data-list">
                  {currentVersion?.steps.map((step) => (
                    <div className="data-row" key={step.key}>
                      <div>
                        <strong>{step.name}</strong>
                        <p className="muted">
                          {step.key} | {step.type}
                        </p>
                      </div>
                      <div className="data-row__meta">
                        <span>{step.assigneeRule ?? "No assignee rule"}</span>
                        <span>
                          {Math.round(step.positionX)}, {Math.round(step.positionY)}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </article>

              <article className="panel panel-stack">
                <header className="section-header">
                  <div>
                    <h3>Current transitions</h3>
                    <p className="muted">Conditions are shown exactly as they would be saved in the workflow schema.</p>
                  </div>
                </header>

                <div className="data-list">
                  {currentVersion?.transitions.map((transition, index) => (
                    <div className="data-row" key={`${transition.from}-${transition.to}-${index}`}>
                      <div>
                        <strong>
                          {transition.from} to {transition.to}
                        </strong>
                        <p className="muted">
                          Condition: {transition.condition ?? "Always"}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </article>
            </div>

            <article className="panel panel-stack">
              <header className="section-header">
                <div>
                  <h3>Schema preview</h3>
                  <p className="muted">Use this page to review the current definition before opening the builder.</p>
                </div>
              </header>

              <pre className="code-panel">
                <code>{JSON.stringify(schemaPreview, null, 2)}</code>
              </pre>
            </article>
          </div>
        </>
      )}
    </section>
  );
}
