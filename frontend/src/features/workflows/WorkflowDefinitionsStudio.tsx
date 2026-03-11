import { useEffect, useState, type DragEvent } from "react";
import { Link } from "react-router-dom";
import {
  addEdge,
  applyEdgeChanges,
  applyNodeChanges,
  Background,
  BackgroundVariant,
  Controls,
  Handle,
  MarkerType,
  MiniMap,
  Position,
  ReactFlow,
  type Connection,
  type Edge,
  type EdgeChange,
  type Node,
  type NodeChange,
  type NodeProps,
  type ReactFlowInstance,
} from "@xyflow/react";
import {
  activateWorkflowVersion,
  createNextWorkflowVersion,
  getWorkflowDefinition,
  updateWorkflowDraft,
  type WorkflowDefinitionDetail,
} from "../../lib/api";

type WorkflowBuilderNodeData = {
  key: string;
  name: string;
  type: string;
  assigneeRule: string;
};

type WorkflowBuilderEdgeData = {
  condition: string;
};

type WorkflowDefinitionsStudioProps = {
  workflowId: string;
};

type WorkflowBuilderNode = Node<WorkflowBuilderNodeData>;
type WorkflowBuilderEdge = Edge<WorkflowBuilderEdgeData>;

const stepTypeOptions = ["start", "task", "approval", "condition", "end"];

function WorkflowStepNode({ data, selected }: NodeProps<WorkflowBuilderNode>) {
  const canReceive = data.type !== "start";
  const canSend = data.type !== "end";

  return (
    <div className={selected ? "workflow-node workflow-node--selected" : "workflow-node"}>
      {canReceive ? <Handle type="target" position={Position.Left} /> : null}
      <span className={`workflow-node__type workflow-node__type--${data.type}`}>{data.type}</span>
      <strong>{data.name}</strong>
      <span className="workflow-node__key">{data.key}</span>
      {data.assigneeRule ? <span className="workflow-node__assignee">{data.assigneeRule}</span> : null}
      {canSend ? <Handle type="source" position={Position.Right} /> : null}
    </div>
  );
}

const nodeTypes = {
  workflowStep: WorkflowStepNode,
};

function createNodeId(key: string, index: number) {
  return `${key}-${index}-${Math.random().toString(36).slice(2, 8)}`;
}

function createUniqueStepKey(nodes: WorkflowBuilderNode[], prefix: string) {
  let counter = nodes.length + 1;
  let candidate = `${prefix}-${counter}`;

  while (nodes.some((node) => node.data.key === candidate)) {
    counter += 1;
    candidate = `${prefix}-${counter}`;
  }

  return candidate;
}

function createNodeFromType(
  type: string,
  position: { x: number; y: number },
  nodes: WorkflowBuilderNode[],
) {
  const key = createUniqueStepKey(nodes, type === "approval" ? "approval" : "step");
  const label = type
    .split("-")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");

  return {
    id: createNodeId(key, nodes.length + 1),
    type: "workflowStep",
    position,
    data: {
      key,
      name: label,
      type,
      assigneeRule: type === "approval" ? "role:manager" : "",
    },
  } satisfies WorkflowBuilderNode;
}

function mapDefinitionToFlow(definition: WorkflowDefinitionDetail | null) {
  if (!definition) {
    return {
      nodes: [] as WorkflowBuilderNode[],
      edges: [] as WorkflowBuilderEdge[],
    };
  }

  const sourceVersion = definition.draftVersion ?? definition.activeVersion;
  if (!sourceVersion) {
    return {
      nodes: [] as WorkflowBuilderNode[],
      edges: [] as WorkflowBuilderEdge[],
    };
  }

  const nodeIdsByKey = new Map<string, string>();

  const nodes = sourceVersion.steps
    .slice()
    .sort((left, right) => left.sortOrder - right.sortOrder)
    .map((step, index) => {
      const nodeId = createNodeId(step.key, index);
      nodeIdsByKey.set(step.key, nodeId);

      return {
        id: nodeId,
        type: "workflowStep",
        position: {
          x: step.positionX,
          y: step.positionY,
        },
        data: {
          key: step.key,
          name: step.name,
          type: step.type,
          assigneeRule: step.assigneeRule ?? "",
        },
      } satisfies WorkflowBuilderNode;
    });

  const edges = sourceVersion.transitions
    .slice()
    .sort((left, right) => left.sortOrder - right.sortOrder)
    .reduce<WorkflowBuilderEdge[]>((current, transition, index) => {
      const source = nodeIdsByKey.get(transition.from);
      const target = nodeIdsByKey.get(transition.to);

      if (!source || !target) {
        return current;
      }

      current.push({
        id: `edge-${index}-${transition.from}-${transition.to}`,
        source,
        target,
        label: transition.condition ?? "",
        data: {
          condition: transition.condition ?? "",
        },
        markerEnd: {
          type: MarkerType.ArrowClosed,
        },
      });

      return current;
    }, []);

  return { nodes, edges };
}

function validateBuilderState(nodes: WorkflowBuilderNode[], edges: WorkflowBuilderEdge[]) {
  if (nodes.length === 0) {
    return "Add at least one workflow step.";
  }

  const duplicateKey = nodes
    .map((node) => node.data.key.trim())
    .find((key, index, keys) => key !== "" && keys.indexOf(key) !== index);

  if (duplicateKey) {
    return `Workflow step key '${duplicateKey}' is duplicated.`;
  }

  if (nodes.some((node) => !node.data.key.trim() || !node.data.name.trim())) {
    return "Every workflow step must have a key and a name.";
  }

  const startCount = nodes.filter((node) => node.data.type === "start").length;
  if (startCount !== 1) {
    return "The workflow must contain exactly one start step.";
  }

  const endCount = nodes.filter((node) => node.data.type === "end").length;
  if (endCount === 0) {
    return "The workflow must contain at least one end step.";
  }

  if (edges.length === 0) {
    return "Add at least one transition between workflow steps.";
  }

  const nodeIds = new Set(nodes.map((node) => node.id));
  if (edges.some((edge) => !nodeIds.has(edge.source) || !nodeIds.has(edge.target))) {
    return "One or more transitions reference a missing workflow step.";
  }

  if (edges.some((edge) => edge.source === edge.target)) {
    return "A workflow step cannot transition to itself.";
  }

  const duplicateTransition = edges.find((edge, index) =>
    edges.findIndex((candidate) => {
      const candidateCondition = candidate.data?.condition?.trim() ?? "";
      const edgeCondition = edge.data?.condition?.trim() ?? "";

      return (
        candidate.source === edge.source &&
        candidate.target === edge.target &&
        candidateCondition.toLowerCase() === edgeCondition.toLowerCase()
      );
    }) !== index,
  );

  if (duplicateTransition) {
    return "Duplicate transitions are not allowed between the same steps with the same condition.";
  }

  const startNode = nodes.find((node) => node.data.type === "start") ?? null;
  if (!startNode) {
    return "The workflow must contain exactly one start step.";
  }

  if (edges.some((edge) => edge.target === startNode.id)) {
    return "The start step cannot receive incoming transitions.";
  }

  const endNodeIds = new Set(
    nodes.filter((node) => node.data.type === "end").map((node) => node.id),
  );

  if (edges.some((edge) => endNodeIds.has(edge.source))) {
    return "End steps cannot have outgoing transitions.";
  }

  const outgoingByNodeId = new Map(nodes.map((node) => [node.id, [] as string[]]));
  const incomingByNodeId = new Map(nodes.map((node) => [node.id, [] as string[]]));

  for (const edge of edges) {
    outgoingByNodeId.get(edge.source)?.push(edge.target);
    incomingByNodeId.get(edge.target)?.push(edge.source);
  }

  const reachableFromStart = traverseGraph(startNode.id, outgoingByNodeId);
  if (reachableFromStart.size !== nodes.length) {
    const unreachable = nodes
      .filter((node) => !reachableFromStart.has(node.id))
      .map((node) => node.data.name || node.data.key);

    return `Every step must be reachable from the start step. Unreachable: ${unreachable.join(", ")}.`;
  }

  const reachableFromEnd = new Set<string>();
  for (const endNodeId of endNodeIds) {
    for (const nodeId of traverseGraph(endNodeId, incomingByNodeId)) {
      reachableFromEnd.add(nodeId);
    }
  }

  if (reachableFromEnd.size !== nodes.length) {
    const deadEnds = nodes
      .filter((node) => !reachableFromEnd.has(node.id))
      .map((node) => node.data.name || node.data.key);

    return `Every step must eventually reach an end step. Dead ends: ${deadEnds.join(", ")}.`;
  }

  return null;
}

function traverseGraph(origin: string, adjacency: Map<string, string[]>) {
  const visited = new Set<string>([origin]);
  const queue = [origin];

  while (queue.length > 0) {
    const current = queue.shift();
    if (!current) {
      continue;
    }

    for (const next of adjacency.get(current) ?? []) {
      if (visited.has(next)) {
        continue;
      }

      visited.add(next);
      queue.push(next);
    }
  }

  return visited;
}

function buildSchemaPreview(
  definition: WorkflowDefinitionDetail | null,
  nodes: WorkflowBuilderNode[],
  edges: WorkflowBuilderEdge[],
) {
  if (!definition) {
    return null;
  }

  const version =
    definition.draftVersion?.versionNumber ?? definition.activeVersion?.versionNumber ?? 0;

  const orderedNodes = nodes
    .slice()
    .sort((left, right) =>
      left.position.y === right.position.y
        ? left.position.x - right.position.x
        : left.position.y - right.position.y,
    );

  const keyByNodeId = new Map(orderedNodes.map((node) => [node.id, node.data.key.trim()]));

  return {
    name: definition.name,
    code: definition.code,
    version,
    trigger: {
      type: "manual",
    },
    steps: orderedNodes.map((node) => ({
      key: node.data.key.trim(),
      name: node.data.name.trim(),
      type: node.data.type,
      position: {
        x: Math.round(node.position.x),
        y: Math.round(node.position.y),
      },
      ...(node.data.assigneeRule.trim() ? { assigneeRule: node.data.assigneeRule.trim() } : {}),
    })),
    transitions: edges.map((edge) => ({
      from: keyByNodeId.get(edge.source),
      to: keyByNodeId.get(edge.target),
      ...(edge.data?.condition?.trim() ? { condition: edge.data.condition.trim() } : {}),
    })),
  };
}

export function WorkflowDefinitionsStudio({ workflowId }: WorkflowDefinitionsStudioProps) {
  const [reactFlowInstance, setReactFlowInstance] = useState<
    ReactFlowInstance<WorkflowBuilderNode, WorkflowBuilderEdge> | null
  >(null);
  const [selectedDefinition, setSelectedDefinition] = useState<WorkflowDefinitionDetail | null>(null);
  const [nodes, setNodes] = useState<WorkflowBuilderNode[]>([]);
  const [edges, setEdges] = useState<WorkflowBuilderEdge[]>([]);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [selectedEdgeId, setSelectedEdgeId] = useState<string | null>(null);
  const [isDetailLoading, setIsDetailLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    void loadWorkflow(workflowId);
  }, [workflowId]);

  useEffect(() => {
    const flowState = mapDefinitionToFlow(selectedDefinition);
    setNodes(flowState.nodes);
    setEdges(flowState.edges);
    setSelectedNodeId(null);
    setSelectedEdgeId(null);
  }, [selectedDefinition]);

  useEffect(() => {
    if (!reactFlowInstance || nodes.length === 0) {
      return;
    }

    const handle = window.setTimeout(() => {
      reactFlowInstance.fitView({ padding: 0.18, duration: 250 });
    }, 80);

    return () => window.clearTimeout(handle);
  }, [reactFlowInstance, selectedDefinition?.id, nodes.length]);

  async function loadWorkflow(id: string) {
    setIsDetailLoading(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      const definition = await getWorkflowDefinition(id);
      setSelectedDefinition(definition);
    } catch (error) {
      setSelectedDefinition(null);
      setErrorMessage(error instanceof Error ? error.message : "Unable to load workflow detail.");
    } finally {
      setIsDetailLoading(false);
    }
  }

  async function handleSaveDraft() {
    if (!selectedDefinition?.draftVersion) {
      return;
    }

    const validationError = validateBuilderState(nodes, edges);
    if (validationError) {
      setErrorMessage(validationError);
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    const orderedNodes = nodes
      .slice()
      .sort((left, right) =>
        left.position.y === right.position.y
          ? left.position.x - right.position.x
          : left.position.y - right.position.y,
      );

    const keyByNodeId = new Map(orderedNodes.map((node) => [node.id, node.data.key.trim()]));

    try {
      const updated = await updateWorkflowDraft(selectedDefinition.id, {
        name: selectedDefinition.name,
        code: selectedDefinition.code,
        expectedRevision: selectedDefinition.revision,
        steps: orderedNodes.map((node) => ({
          key: node.data.key.trim(),
          name: node.data.name.trim(),
          type: node.data.type,
          assigneeRule: node.data.assigneeRule.trim() || null,
          positionX: node.position.x,
          positionY: node.position.y,
        })),
        transitions: edges.map((edge) => ({
          from: keyByNodeId.get(edge.source) ?? "",
          to: keyByNodeId.get(edge.target) ?? "",
          condition: edge.data?.condition?.trim() || null,
        })),
      });

      setSelectedDefinition(updated);
      setSuccessMessage(`Saved draft version ${updated.draftVersion?.versionNumber ?? ""}.`);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Unable to save workflow draft.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleCreateDraftVersion() {
    if (!selectedDefinition) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      const updated = await createNextWorkflowVersion(
        selectedDefinition.id,
        selectedDefinition.revision,
      );
      setSelectedDefinition(updated);
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
    if (!selectedDefinition?.draftVersion) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      const updated = await activateWorkflowVersion(
        selectedDefinition.id,
        selectedDefinition.draftVersion.versionNumber,
        selectedDefinition.revision,
      );

      setSelectedDefinition(updated);
      setSuccessMessage(`Activated version ${selectedDefinition.draftVersion.versionNumber}.`);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Unable to activate draft.");
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleNodesChange(changes: NodeChange<WorkflowBuilderNode>[]) {
    setNodes((current) => applyNodeChanges(changes, current));
  }

  function handleEdgesChange(changes: EdgeChange<WorkflowBuilderEdge>[]) {
    setEdges((current) => applyEdgeChanges(changes, current));
  }

  function handleConnect(connection: Connection) {
    if (!selectedDefinition?.draftVersion || !connection.source || !connection.target) {
      return;
    }

    setEdges((current) =>
      addEdge(
        {
          ...connection,
          id: `edge-${connection.source}-${connection.target}-${current.length + 1}`,
          label: "",
          data: {
            condition: "",
          },
          markerEnd: {
            type: MarkerType.ArrowClosed,
          },
        },
        current,
      ),
    );
  }

  function handleNodeDrop(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();

    if (!selectedDefinition?.draftVersion || !reactFlowInstance) {
      return;
    }

    const stepType = event.dataTransfer.getData("application/x-workflow-step");
    if (!stepType) {
      return;
    }

    const position = reactFlowInstance.screenToFlowPosition({
      x: event.clientX,
      y: event.clientY,
    });

    setNodes((current) => [...current, createNodeFromType(stepType, position, current)]);
    setSuccessMessage(`Added ${stepType} step to the workflow canvas.`);
    setSelectedEdgeId(null);
  }

  function updateSelectedNodeField(
    field: keyof WorkflowBuilderNodeData,
    value: string,
  ) {
    if (!selectedNodeId) {
      return;
    }

    setNodes((current) =>
      current.map((node) =>
        node.id === selectedNodeId
          ? {
              ...node,
              data: {
                ...node.data,
                [field]: value,
              },
            }
          : node,
      ),
    );
  }

  function updateSelectedEdgeCondition(value: string) {
    if (!selectedEdgeId) {
      return;
    }

    setEdges((current) =>
      current.map((edge) =>
        edge.id === selectedEdgeId
          ? {
              ...edge,
              label: value,
              data: {
                condition: value,
              },
            }
          : edge,
      ),
    );
  }

  function removeSelectedNode() {
    if (!selectedNodeId || !selectedDefinition?.draftVersion) {
      return;
    }

    setNodes((current) => current.filter((node) => node.id !== selectedNodeId));
    setEdges((current) =>
      current.filter((edge) => edge.source !== selectedNodeId && edge.target !== selectedNodeId),
    );
    setSelectedNodeId(null);
  }

  function removeSelectedEdge() {
    if (!selectedEdgeId || !selectedDefinition?.draftVersion) {
      return;
    }

    setEdges((current) => current.filter((edge) => edge.id !== selectedEdgeId));
    setSelectedEdgeId(null);
  }

  const selectedNode = nodes.find((node) => node.id === selectedNodeId) ?? null;
  const selectedEdge = edges.find((edge) => edge.id === selectedEdgeId) ?? null;
  const schemaPreview = buildSchemaPreview(selectedDefinition, nodes, edges);

  return (
    <section className="panel-stack">
      <header className="page-header">
        <div>
          <p className="page-header__eyebrow">Workflow builder</p>
          <h2 className="page-header__title">
            {selectedDefinition?.name ?? (isDetailLoading ? "Loading builder..." : "Workflow builder")}
          </h2>
          <p className="muted">Use the dedicated builder page to edit one workflow at a time without mixing catalog actions into the editor.</p>
        </div>
        <div className="button-row">
          <Link className="button button--secondary" to="/workflows">
            Catalog
          </Link>
          {selectedDefinition ? (
            <Link className="button button--secondary" to={`/workflows/${selectedDefinition.id}`}>
              Overview
            </Link>
          ) : null}
        </div>
      </header>

      {errorMessage ? <div className="notice notice--error">{errorMessage}</div> : null}
      {successMessage ? <div className="notice notice--success">{successMessage}</div> : null}

      {!selectedDefinition ? (
        <article className="panel empty-state">
          <strong>{isDetailLoading ? "Loading workflow..." : "Workflow unavailable"}</strong>
          <p>
            {isDetailLoading
              ? "The builder is loading the selected workflow."
              : "The selected workflow could not be loaded. Return to the catalog and choose another workflow."}
          </p>
        </article>
      ) : (
        <>
          <article className="panel panel-stack">
            <header className="section-header">
              <div>
                <h3>{selectedDefinition.name}</h3>
                <p className="muted">
                  {selectedDefinition.code} | {selectedDefinition.draftVersion ? "Draft open" : "Read-only active version"}
                </p>
              </div>

              <div className="button-row">
                <button
                  className="button button--secondary"
                  disabled={isSubmitting || Boolean(selectedDefinition.draftVersion)}
                  onClick={() => void handleCreateDraftVersion()}
                  type="button"
                >
                  New draft version
                </button>
                <button
                  className="button"
                  disabled={isSubmitting || !selectedDefinition.draftVersion}
                  onClick={() => void handleSaveDraft()}
                  type="button"
                >
                  Save draft
                </button>
                <button
                  className="button button--accent"
                  disabled={isSubmitting || !selectedDefinition.draftVersion}
                  onClick={() => void handleActivateDraft()}
                  type="button"
                >
                  Activate draft
                </button>
              </div>
            </header>

            <div className="version-list">
              {selectedDefinition.versions.map((version) => (
                <div className="version-pill" key={version.versionNumber}>
                  <strong>v{version.versionNumber}</strong>
                  <span>{version.status}</span>
                  <span>{version.stepCount} steps</span>
                  <span>{version.transitionCount} transitions</span>
                </div>
              ))}
            </div>
          </article>

          <div className="builder-layout">
            <article className="panel panel-stack">
              <header className="section-header">
                <div>
                  <h3>Builder palette</h3>
                  <p className="muted">Drag step cards onto the canvas, connect them, then save the draft.</p>
                </div>
              </header>

              <div className="palette-grid">
                {stepTypeOptions.map((type) => (
                  <button
                    key={type}
                    className={`palette-card palette-card--${type}`}
                    disabled={!selectedDefinition.draftVersion}
                    draggable={Boolean(selectedDefinition.draftVersion)}
                    onDragStart={(event) => {
                      event.dataTransfer.setData("application/x-workflow-step", type);
                      event.dataTransfer.effectAllowed = "move";
                    }}
                    onClick={() => {
                      if (!selectedDefinition.draftVersion) {
                        return;
                      }

                      setNodes((current) => [
                        ...current,
                        createNodeFromType(
                          type,
                          { x: 160 + current.length * 28, y: 120 + current.length * 18 },
                          current,
                        ),
                      ]);
                    }}
                    type="button"
                  >
                    <span className="palette-card__type">{type}</span>
                    <strong>{type.replace("-", " ")}</strong>
                  </button>
                ))}
              </div>

              <div className="builder-canvas">
                {isDetailLoading ? <p className="muted">Loading workflow detail...</p> : null}
                <ReactFlow<WorkflowBuilderNode, WorkflowBuilderEdge>
                  nodes={nodes}
                  edges={edges}
                  nodeTypes={nodeTypes}
                  onInit={(instance) => setReactFlowInstance(instance)}
                  onNodesChange={handleNodesChange}
                  onEdgesChange={handleEdgesChange}
                  onConnect={handleConnect}
                  onDrop={handleNodeDrop}
                  onDragOver={(event) => {
                    event.preventDefault();
                    event.dataTransfer.dropEffect = "move";
                  }}
                  onNodeClick={(_, node) => {
                    setSelectedNodeId(node.id);
                    setSelectedEdgeId(null);
                  }}
                  onEdgeClick={(_, edge) => {
                    setSelectedEdgeId(edge.id);
                    setSelectedNodeId(null);
                  }}
                  onPaneClick={() => {
                    setSelectedNodeId(null);
                    setSelectedEdgeId(null);
                  }}
                  fitView
                  proOptions={{ hideAttribution: true }}
                >
                  <Background variant={BackgroundVariant.Dots} gap={16} size={1.2} />
                  <Controls />
                  <MiniMap pannable zoomable />
                </ReactFlow>
                {!selectedDefinition.draftVersion ? (
                  <div className="builder-overlay">
                    <strong>Read-only view</strong>
                    <p>Create a new draft version to edit this workflow on the canvas.</p>
                  </div>
                ) : null}
              </div>
            </article>

            <div className="panel-stack">
              <article className="panel panel-stack">
                <header className="section-header">
                  <div>
                    <h3>Workflow properties</h3>
                    <p className="muted">Edit the workflow metadata that ships with this definition.</p>
                  </div>
                </header>

                <label className="field">
                  <span>Workflow name</span>
                  <input
                    className="input"
                    disabled={!selectedDefinition.draftVersion}
                    value={selectedDefinition.name}
                    onChange={(event) =>
                      setSelectedDefinition((current) =>
                        current ? { ...current, name: event.target.value } : current,
                      )
                    }
                  />
                </label>

                <label className="field">
                  <span>Workflow code</span>
                  <input
                    className="input"
                    disabled={!selectedDefinition.draftVersion}
                    value={selectedDefinition.code}
                    onChange={(event) =>
                      setSelectedDefinition((current) =>
                        current ? { ...current, code: event.target.value } : current,
                      )
                    }
                  />
                </label>
              </article>

              <article className="panel panel-stack">
                <header className="section-header">
                  <div>
                    <h3>Selected node</h3>
                    <p className="muted">Click a node to rename it, change its type, or assign work.</p>
                  </div>
                  <button
                    className="button button--ghost"
                    disabled={!selectedDefinition.draftVersion || !selectedNode}
                    onClick={removeSelectedNode}
                    type="button"
                  >
                    Delete node
                  </button>
                </header>

                {!selectedNode ? (
                  <p className="muted">No node selected.</p>
                ) : (
                  <>
                    <label className="field">
                      <span>Step key</span>
                      <input
                        className="input"
                        disabled={!selectedDefinition.draftVersion}
                        value={selectedNode.data.key}
                        onChange={(event) => updateSelectedNodeField("key", event.target.value)}
                      />
                    </label>
                    <label className="field">
                      <span>Step name</span>
                      <input
                        className="input"
                        disabled={!selectedDefinition.draftVersion}
                        value={selectedNode.data.name}
                        onChange={(event) => updateSelectedNodeField("name", event.target.value)}
                      />
                    </label>
                    <label className="field">
                      <span>Step type</span>
                      <select
                        className="input"
                        disabled={!selectedDefinition.draftVersion}
                        value={selectedNode.data.type}
                        onChange={(event) => updateSelectedNodeField("type", event.target.value)}
                      >
                        {stepTypeOptions.map((option) => (
                          <option key={option} value={option}>
                            {option}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="field">
                      <span>Assignee rule</span>
                      <input
                        className="input"
                        disabled={!selectedDefinition.draftVersion}
                        value={selectedNode.data.assigneeRule}
                        onChange={(event) => updateSelectedNodeField("assigneeRule", event.target.value)}
                        placeholder="role:manager or team:operations"
                      />
                    </label>
                    <p className="muted">
                      Position: {Math.round(selectedNode.position.x)}, {Math.round(selectedNode.position.y)}
                    </p>
                  </>
                )}
              </article>

              <article className="panel panel-stack">
                <header className="section-header">
                  <div>
                    <h3>Selected transition</h3>
                    <p className="muted">Connect nodes on the canvas, then select an edge to add a condition.</p>
                  </div>
                  <button
                    className="button button--ghost"
                    disabled={!selectedDefinition.draftVersion || !selectedEdge}
                    onClick={removeSelectedEdge}
                    type="button"
                  >
                    Delete transition
                  </button>
                </header>

                {!selectedEdge ? (
                  <p className="muted">No transition selected.</p>
                ) : (
                  <>
                    <p className="muted">
                      {nodes.find((node) => node.id === selectedEdge.source)?.data.key} to{" "}
                      {nodes.find((node) => node.id === selectedEdge.target)?.data.key}
                    </p>
                    <label className="field">
                      <span>Condition</span>
                      <input
                        className="input"
                        disabled={!selectedDefinition.draftVersion}
                        value={selectedEdge.data?.condition ?? ""}
                        onChange={(event) => updateSelectedEdgeCondition(event.target.value)}
                        placeholder="approved, rejected, high-value"
                      />
                    </label>
                  </>
                )}
              </article>

              <article className="panel panel-stack">
                <header className="section-header">
                  <div>
                    <h3>Schema preview</h3>
                    <p className="muted">The saved JSON snapshot generated from the current builder state.</p>
                  </div>
                </header>

                <pre className="code-panel">
                  <code>{JSON.stringify(schemaPreview, null, 2)}</code>
                </pre>
              </article>
            </div>
          </div>
        </>
      )}
    </section>
  );
}
