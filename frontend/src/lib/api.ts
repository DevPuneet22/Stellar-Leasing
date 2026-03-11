import { config } from "./config";
import { clearStoredSession, getStoredSession, type AuthSession } from "../features/auth/authStorage";

export type WorkflowDefinitionSummary = {
  id: string;
  name: string;
  code: string;
  draftVersionNumber: number;
  activeVersionNumber: number | null;
};

export type WorkflowStep = {
  key: string;
  name: string;
  type: string;
  assigneeRule: string | null;
  sortOrder: number;
  positionX: number;
  positionY: number;
};

export type WorkflowTransition = {
  from: string;
  to: string;
  condition: string | null;
  sortOrder: number;
};

export type WorkflowVersionDetail = {
  versionNumber: number;
  status: string;
  steps: WorkflowStep[];
  transitions: WorkflowTransition[];
};

export type WorkflowVersionSummary = {
  versionNumber: number;
  status: string;
  stepCount: number;
  transitionCount: number;
};

export type WorkflowDefinitionDetail = {
  id: string;
  name: string;
  code: string;
  revision: number;
  draftVersion: WorkflowVersionDetail | null;
  activeVersion: WorkflowVersionDetail | null;
  versions: WorkflowVersionSummary[];
};

export type CreateWorkflowDefinitionPayload = {
  name: string;
  code?: string | null;
};

export type WorkflowStepInput = {
  key: string;
  name: string;
  type: string;
  assigneeRule?: string | null;
  positionX: number;
  positionY: number;
};

export type WorkflowTransitionInput = {
  from: string;
  to: string;
  condition?: string | null;
};

export type UpdateWorkflowDraftPayload = {
  name: string;
  code?: string | null;
  expectedRevision: number;
  steps: WorkflowStepInput[];
  transitions: WorkflowTransitionInput[];
};

export type LoginPayload = {
  email: string;
  password: string;
};

function buildHeaders(init?: RequestInit) {
  const headers = new Headers(init?.headers);
  const session = getStoredSession();

  if (!headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (session) {
    headers.set("Authorization", `Bearer ${session.accessToken}`);
    headers.set("X-Tenant-Id", session.tenantId);
  } else {
    headers.set("X-Tenant-Id", config.tenantId);
  }

  return headers;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${config.apiBaseUrl}${path}`, {
    headers: buildHeaders(init),
    ...init,
  });

  const contentType = response.headers.get("content-type");
  const body =
    contentType?.includes("application/json") === true ? await response.json() : await response.text();

  if (!response.ok) {
    if (response.status === 401) {
      clearStoredSession();
      if (window.location.pathname !== "/login") {
        window.location.assign("/login");
      }
    }

    const message =
      typeof body === "object" && body !== null
        ? String(body.detail ?? body.title ?? "Request failed.")
        : String(body || "Request failed.");

    throw new Error(message);
  }

  return body as T;
}

export function login(payload: LoginPayload) {
  return request<AuthSession>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function listWorkflowDefinitions() {
  return request<WorkflowDefinitionSummary[]>("/api/workflow-definitions");
}

export function getWorkflowDefinition(id: string) {
  return request<WorkflowDefinitionDetail>(`/api/workflow-definitions/${id}`);
}

export function createWorkflowDefinition(payload: CreateWorkflowDefinitionPayload) {
  return request<WorkflowDefinitionDetail>("/api/workflow-definitions", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function updateWorkflowDraft(id: string, payload: UpdateWorkflowDraftPayload) {
  return request<WorkflowDefinitionDetail>(`/api/workflow-definitions/${id}/draft`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function createNextWorkflowVersion(id: string, expectedRevision: number) {
  return request<WorkflowDefinitionDetail>(`/api/workflow-definitions/${id}/versions`, {
    method: "POST",
    body: JSON.stringify({ expectedRevision }),
  });
}

export function activateWorkflowVersion(id: string, versionNumber: number, expectedRevision: number) {
  return request<WorkflowDefinitionDetail>(`/api/workflow-definitions/${id}/activate`, {
    method: "POST",
    body: JSON.stringify({ versionNumber, expectedRevision }),
  });
}
