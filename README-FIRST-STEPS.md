# Stellar Leasing First Steps

This file is the practical build order for the current scaffold.

If you follow only one rule, follow this one:

Build the workflow engine and admin CRUD first. Do not build the drag-and-drop builder first.

## What We Are Building

Stellar Leasing is a workflow automation product.

The first real use case is lease approval, but the product itself should be reusable for other workflows later.

That means the MVP should let an admin:

- define a workflow
- version the workflow
- assign steps to users, roles, or teams
- run the workflow on real records
- approve or reject work
- keep comments, attachments, and audit history

## Current State

What already exists:

- backend solution with `Domain`, `Application`, `Infrastructure`, `Api`, and `Worker`
- starter workflow definition domain model
- in-memory workflow definition repository
- starter API endpoints
- starter worker process
- React frontend shell with dashboard, docs, and builder preview pages
- PostgreSQL Docker setup

What does not exist yet:

- real database persistence through EF Core
- authentication
- tenant isolation
- roles and teams
- workflow definition CRUD UI
- runtime process execution
- task inbox
- comments, attachments, reminders, escalations
- visual builder

## Build Order

Do the work in this order:

1. move from in-memory data to EF Core + PostgreSQL
2. add auth, tenant, role, and team foundations
3. finish workflow definition APIs
4. build workflow definition CRUD in the frontend
5. build runtime process execution
6. build task inbox and task actions
7. add audit, comments, attachments, reminders, and escalations
8. build the visual workflow builder last

## Step 1: Replace In-Memory Storage

This is the first real implementation step.

Why first:

- nothing important can become stable until data is stored properly
- auth, runtime, inbox, and audit all depend on persistent data
- the current repository resets on every restart

Build:

- add EF Core packages
- create `DbContext`
- map `WorkflowDefinition`, `WorkflowVersion`, `WorkflowStep`, and `WorkflowTransition`
- replace the in-memory repository with a PostgreSQL-backed repository
- add the first migration
- load the connection string from configuration

Start with these files:

- `backend/src/StellarLeasing.Infrastructure/DependencyInjection.cs`
- `backend/src/StellarLeasing.Infrastructure/Persistence/InMemoryWorkflowDefinitionRepository.cs`
- `backend/src/StellarLeasing.Domain/WorkflowDefinitions/WorkflowDefinition.cs`
- `backend/src/StellarLeasing.Api/Program.cs`
- `.env.example`

New files you will likely add:

- `backend/src/StellarLeasing.Infrastructure/Persistence/StellarLeasingDbContext.cs`
- `backend/src/StellarLeasing.Infrastructure/Persistence/Configurations/...`
- `backend/src/StellarLeasing.Infrastructure/Persistence/PostgresWorkflowDefinitionRepository.cs`

Done means:

- `GET /api/workflow-definitions` returns data from PostgreSQL
- `POST /api/workflow-definitions` saves data in PostgreSQL
- after restarting the API, created workflows still exist

## Step 2: Add Auth, Tenant, Roles, and Teams

Why second:

- workflow assignment is meaningless without users
- multi-tenant boundaries should exist before runtime data grows
- permissions should be part of the shape early, not bolted on later

Build:

- add users
- add tenant entity persistence
- add roles and teams
- add login endpoint
- issue JWT tokens
- secure workflow endpoints
- resolve the current tenant from the user context

Start with these areas:

- `backend/src/StellarLeasing.Domain/Tenants`
- `backend/src/StellarLeasing.Api/Program.cs`
- `docs/domain-model.md`
- `docs/api-spec.md`

Done means:

- a user can log in
- protected API endpoints require a token
- workflow definitions are isolated by tenant

## Step 3: Finish Workflow Definition APIs

Right now the workflow definition API is only a starter.

Build:

- get workflow definition by id
- create new version
- activate a version
- update draft version
- list steps and transitions
- validate duplicate codes and invalid transitions

Important rule:

- never edit an active workflow version in place

Start with these files:

- `backend/src/StellarLeasing.Api/Controllers/WorkflowDefinitionsController.cs`
- `backend/src/StellarLeasing.Application/WorkflowDefinitions/...`
- `docs/workflow-schema.md`
- `docs/api-spec.md`

Done means:

- admins can fully manage versioned workflow definitions through the API
- active versions remain immutable

## Step 4: Build Workflow Definition CRUD In The Frontend

Do this before the visual builder.

Build:

- workflow definitions list page
- create workflow form
- draft version editor using normal forms and tables
- version activation action
- raw JSON inspection view

Do not build:

- drag-and-drop canvas
- graph editing UI

Why:

- the form-based editor is faster to build
- it proves the schema and backend rules
- it reduces rework before the builder exists

Start with these files:

- `frontend/src/app/App.tsx`
- `frontend/src/components/layout/AppShell.tsx`
- `frontend/src/pages/...`
- `frontend/src/features/...`
- `frontend/src/lib/config.ts`

Done means:

- an admin can create and manage workflow definitions from the frontend
- the frontend uses the real API, not hardcoded mock numbers

## Step 5: Build Runtime Process Execution

This is where the product becomes real.

Build:

- start a process instance from an active workflow version
- create current step instances
- assign tasks based on user, role, or team rules
- approve or reject steps
- evaluate transitions
- move to next step
- mark the process complete

Keep this rule:

- runtime data must reference a version snapshot
- runtime data must stay separate from definition data

Suggested new domain areas:

- `ProcessInstance`
- `ProcessStepInstance`
- `TaskItem`
- `ApprovalDecision`

Done means:

- a workflow can actually run from start to finish in the backend

## Step 6: Build The Task Inbox

Once runtime exists, users need a place to work.

Build:

- my tasks
- team tasks
- due soon
- overdue
- task details
- approve and reject actions
- activity timeline

Frontend goal:

- stop showing placeholder dashboard metrics
- show real task and workflow counts from the backend

Done means:

- a user can open the app and complete assigned work

## Step 7: Add Audit, Comments, Attachments, Reminders, and Escalations

These make the system usable in real business flows.

Build:

- append-only audit log
- comments on tasks and process instances
- attachments
- due date tracking
- reminder jobs
- escalation jobs

Use the worker for:

- reminder scans
- overdue detection
- escalation triggers
- email or notification dispatch later

Done means:

- important actions are traceable
- overdue work can be detected without manual checking

## Step 8: Build The Visual Builder

Only do this after the previous steps are stable.

Build:

- node-based workflow editor
- edge editing for transitions
- step configuration panel
- builder save/load against the same workflow schema

Use:

- `React Flow`

Important rule:

- the builder must read and write the same schema already used by the CRUD editor and runtime engine

Done means:

- the visual builder is another editor for the same workflow definition, not a second system

## What To Work On Next

If you want the single next task, do this:

Implement Step 1 and replace the in-memory repository with EF Core + PostgreSQL.

That is the correct next move because almost every other important feature depends on stable persistence.

## Suggested Weekly Path

Week 1:

- finish EF Core persistence
- add migrations
- connect API to PostgreSQL

Week 2:

- add auth
- add tenants, roles, and teams

Week 3:

- complete workflow definition APIs
- validate the schema rules

Week 4:

- build workflow definition CRUD UI

Week 5:

- build runtime process execution

Week 6:

- build task inbox
- add approval and rejection actions

Week 7:

- add comments, attachments, audit, reminders, and escalations

Week 8:

- start the visual builder

## Practical Rule For Every Feature

For each new feature, do the work in this order:

1. update the domain model
2. update persistence
3. update application services
4. expose API endpoints
5. build frontend screens
6. add tests

That order will keep the system coherent and reduce rework.
