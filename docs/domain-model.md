# Domain Model

## Core Entities

- `Tenant`: top-level company boundary
- `User`: authenticated person within a tenant
- `Role`: permission grouping
- `Team`: work assignment grouping
- `WorkflowDefinition`: reusable workflow template
- `WorkflowVersion`: immutable version snapshot of a workflow definition
- `WorkflowStep`: step inside a version
- `WorkflowTransition`: path between steps
- `ProcessInstance`: runtime execution of a workflow version
- `TaskItem`: actionable work item for a user or team
- `ApprovalDecision`: approve/reject outcome with comments
- `Comment`: threaded note on a task or process
- `Attachment`: uploaded evidence or supporting file
- `Notification`: reminder, escalation, or FYI event
- `AuditLog`: immutable event history

## Key Relationships

- one `Tenant` has many `Users`, `Teams`, and `WorkflowDefinitions`
- one `WorkflowDefinition` has many `WorkflowVersions`
- one `WorkflowVersion` has many `WorkflowSteps` and `WorkflowTransitions`
- one `ProcessInstance` points to exactly one `WorkflowVersion`
- one `ProcessInstance` has many `TaskItems`, `Comments`, and `AuditLogs`

## Modeling Rules

- definition data must stay separate from runtime data
- live workflow versions should never be edited in place
- task assignment rules should support user, role, and team targeting
- audit logs should be append-only
