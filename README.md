# Stellar-Leasing

This repository is the starting point for a workflow automation product inspired by Qntrl. The goal is to let businesses define workflows, assign tasks, approve/reject steps, track SLAs, and keep a full audit trail.

## Product Goal

Build a SaaS-style workflow platform where an admin can:

- create a workflow
- define steps, conditions, roles, and approvals
- assign forms to steps
- run the workflow on real business records
- monitor pending work, delays, and status
- keep audit logs, comments, and attachments

Examples:

- lease approval workflow
- procurement workflow
- employee onboarding workflow
- finance approval workflow

## Recommended Stack

Use a modular monolith first. Do not start with microservices.

### Backend

- `ASP.NET Core 10 Web API`
- `Entity Framework Core`
- `PostgreSQL 17+` (`18` is fine if your hosting supports it)
- `Hangfire` for background jobs, reminders, escalations, and retries
- `ASP.NET Core Identity + JWT` for authentication
- `Serilog + OpenTelemetry` for logs and tracing

### Frontend

- `React`
- `TypeScript`
- `Vite` (preferred for this product)
- `React Router`
- `TanStack Query`
- `React Hook Form`
- `Zod`
- `Tailwind CSS`
- `React Flow` for the workflow designer canvas

Optional alternative:

- `Next.js` only if you also want SSR, public landing pages, public workflow intake forms, or a Node-based backend-for-frontend layer

### Testing

- `xUnit` for backend tests
- `Vitest` for frontend unit tests
- `Playwright` for end-to-end tests

### DevOps

- `Docker Compose` for local development
- `GitHub Actions` for CI/CD
- deploy first on `Azure`, `AWS`, or `Render`, depending on budget and comfort

### Qntrl-Visible Alignment

Qntrl's exact private core stack is not publicly documented. Because of that, this project should align with the parts of Qntrl that are publicly visible in their docs, not make unverifiable claims about their internal implementation.

Publicly visible Qntrl technology patterns:

- `JavaScript/TypeScript` style browser customization
- `JavaScript` server-side scripting for automation extensions
- `ES modules` for reusable script modules
- a manifest-based widget/plugin model
- an optional `Java` bridge/agent model for on-prem or legacy integrations

How to apply that here:

- keep the main product on `React + TypeScript + Vite`
- keep the core backend on `ASP.NET Core + PostgreSQL`
- add a `JavaScript` extension layer for workflow scripts, widgets, and custom actions
- add a separate `Java` bridge only if enterprise integrations or on-prem connectivity become a real requirement

## Why This Stack

- `ASP.NET Core 10` is a strong choice for a business platform because it is fast, stable, and well suited to APIs, auth, background jobs, and enterprise-style domain logic.
- Microsoft lists `.NET 10` as the current LTS release, supported until `November 14, 2028`.
- `React` has the best ecosystem for rich admin interfaces and node-based editors.
- `React Flow` is the right fit for drag-and-drop workflow design instead of building a graph editor from scratch.
- `PostgreSQL` is a strong default for relational workflows, reporting, filtering, and audit history.
- A modular monolith is faster to build and easier to maintain early on than microservices.
- this approach also stays close to Qntrl's publicly visible technology model by planning for `JavaScript` scripts/widgets and an optional `Java` bridge

## Vite vs Next.js

For this specific product, prefer `Vite`.

### Prefer Vite When

- the app is mainly an authenticated dashboard
- most pages are internal business UI
- the workflow builder is highly interactive and client-heavy
- `ASP.NET Core` is already your real backend
- you want the simplest frontend architecture

### Prefer Next.js When

- you need SEO-driven public marketing pages
- you need public submission portals or intake forms
- you want server-rendered pages
- you want to use Node route handlers, server actions, or a backend-for-frontend layer
- you want one frontend platform to handle both the product UI and the public website

### Recommendation

For `Stellar-Leasing`, a Qntrl-like workflow platform, `Vite + React` is the cleaner default because:

- the main value is inside the authenticated product, not public SEO pages
- the workflow designer is mostly client-side UI
- `ASP.NET Core` already covers the backend concerns well
- using `Next.js` only as a SPA usually adds framework complexity without giving you its strongest benefits

## What Not To Do First

Do not begin with:

- microservices
- a BPMN-complete engine
- a complex drag-and-drop builder before runtime exists
- billing, marketplace, or many integrations
- AI features before the workflow core is reliable

The biggest mistake in products like this is building the visual designer first. Build the workflow engine and schema first, then make the UI generate that schema.

## Recommended Architecture

Start with one backend solution and one frontend app.

### Backend Modules

- `Identity`: users, roles, permissions, teams, tenants
- `Workflow Definitions`: workflow templates, versions, steps, conditions
- `Forms`: form schema, validation rules, field mapping
- `Runtime`: process instances, step execution, task assignment, approvals
- `Notifications`: email, in-app alerts, reminders, escalations
- `Audit`: logs, comments, attachments, history
- `Reporting`: dashboards, filters, SLA metrics

### Frontend Modules

- auth
- admin settings
- workflow list
- workflow builder
- form builder
- task inbox
- workflow run details
- reports dashboard

## Suggested Repo Structure

```text
Stellar-Leasing/
|-- docs/
|   |-- prd.md
|   |-- domain-model.md
|   |-- workflow-schema.md
|   |-- api-spec.md
|   `-- milestones.md
|-- backend/
|   |-- src/
|   |   |-- StellarLeasing.Domain/
|   |   |-- StellarLeasing.Application/
|   |   |-- StellarLeasing.Infrastructure/
|   |   |-- StellarLeasing.Api/
|   |   `-- StellarLeasing.Worker/
|   `-- tests/
|       |-- StellarLeasing.Domain.Tests/
|       |-- StellarLeasing.Application.Tests/
|       `-- StellarLeasing.Api.Tests/
|-- frontend/
|   |-- src/
|   |   |-- app/
|   |   |-- components/
|   |   |-- features/
|   |   |-- pages/
|   |   |-- lib/
|   |   `-- styles/
|   `-- tests/
|-- docker/
|   |-- docker-compose.yml
|   `-- postgres/
`-- README.md
```

## Core Domain Model

These are the main entities you should design early:

- `Tenant`
- `User`
- `Role`
- `Team`
- `WorkflowDefinition`
- `WorkflowVersion`
- `WorkflowStep`
- `WorkflowTransition`
- `FormDefinition`
- `FormField`
- `ProcessInstance`
- `ProcessStepInstance`
- `TaskItem`
- `ApprovalDecision`
- `Comment`
- `Attachment`
- `Notification`
- `AuditLog`
- `SlaRule`

## Step-by-Step Build Path

Follow this order.

### Step 1: Define the MVP Clearly

Create `docs/prd.md` and answer:

- Who is the first user: leasing team, HR team, finance team, or general business admin?
- What are the first 3 workflows to support?
- What is in scope for v1?
- What is explicitly out of scope?

For v1, keep the scope small.

Recommended MVP:

- tenant/company setup
- login and roles
- workflow definition with versioning
- manual approval steps
- conditional branching
- forms on each step
- task inbox
- comments and attachments
- audit trail
- email reminders

Not required for MVP:

- public marketplace
- external integrations
- low-code scripting
- advanced analytics
- no-code AI generation

### Step 2: Set Up the Repository

Create:

- `docs/`
- `backend/`
- `frontend/`
- `docker/`

Then set up:

- backend solution
- frontend app
- PostgreSQL container
- environment files
- CI pipeline

### Step 3: Build Identity and Tenant Basics

Implement first:

- sign in
- tenant/company
- users
- roles
- permissions
- teams

Without this, nothing else will scale correctly.

### Step 4: Design the Workflow Schema

Before any visual builder, define the JSON/data structure for workflows.

A workflow definition should support:

- name
- version
- trigger type
- steps
- step type
- assignee rule
- due date rule
- transitions
- conditions
- form mapping
- escalation rule

This is the most important design step in the whole product.

### Step 5: Build a Simple Admin CRUD for Workflows

Do not build drag-and-drop yet.

Build:

- create workflow
- add steps in a normal form/table UI
- save versions
- activate/deactivate a version
- inspect the raw JSON definition

This gives you fast progress and lets you validate the model before investing in canvas UX.

### Step 6: Build the Runtime Engine

Implement the engine that can:

- start a workflow instance
- create current tasks
- assign tasks to users or roles
- move to next step after approval/rejection
- evaluate conditions
- handle deadlines
- log everything

At this point, your product starts becoming real.

### Step 7: Build the Task Inbox

Users need a place to work.

Build:

- my tasks
- team tasks
- due soon
- overdue
- approve/reject
- comments
- attachments
- activity timeline

### Step 8: Build Form Definitions and Rendering

Each step may need inputs.

Implement:

- form definition schema
- reusable field types
- required/optional rules
- validation
- read-only fields
- field visibility rules
- store submitted values by process instance

### Step 9: Add Notifications and SLA Rules

Add:

- email notifications
- in-app notifications
- due date reminders
- escalation to manager/team
- overdue tracking

### Step 10: Add the Visual Workflow Builder

Now build the drag-and-drop UI with `React Flow`.

Important rule:

- the builder must read and write the same workflow schema you designed earlier

The builder is only another editor for the same definition, not a separate system.

### Step 11: Add Reporting and Audit

Implement:

- workflow status dashboards
- average completion time
- pending by assignee
- overdue by workflow
- full audit history
- exportable logs

### Step 12: Add Integrations

Only after the core works well, add:

- email providers
- webhooks
- ERP/CRM integrations
- document storage
- third-party identity providers

## First 8 Concrete Tasks

If you are starting from zero, do these first:

1. write `docs/prd.md`
2. write `docs/domain-model.md`
3. write `docs/workflow-schema.md`
4. create the backend solution and projects
5. create the frontend app
6. run PostgreSQL with Docker
7. implement auth + tenant + roles
8. implement workflow definition CRUD without drag-and-drop

## Recommended MVP Release Plan

### Milestone 1: Foundation

- auth
- tenants
- roles
- teams
- base layout
- database setup

### Milestone 2: Workflow Definition

- workflow templates
- versioning
- steps
- transitions
- conditions

### Milestone 3: Runtime

- start instance
- assign tasks
- approve/reject
- comments
- history

### Milestone 4: Usability

- forms
- notifications
- SLAs
- inbox filters

### Milestone 5: Builder and Reports

- visual builder
- dashboards
- exports

## Technical Rules To Keep

- use `versioned workflow definitions`
- never edit a live workflow version in place
- keep `runtime data` separate from `definition data`
- log every important action in `AuditLog`
- keep permissions explicit
- use background jobs for reminders and escalations
- avoid premature generic abstractions

## Best Starting Choice

If you want one direct answer: use `React + TypeScript + Vite` for the frontend and `ASP.NET Core 10 + PostgreSQL` for the backend, and build it as a `modular monolith`.

That is the best balance of:

- development speed
- long-term maintainability
- enterprise readiness
- support for a visual workflow builder

Choose `Next.js` instead only if public SSR pages, public intake flows, or a Node BFF are real product requirements from the start.

To stay close to Qntrl, add:

- a `JavaScript` scripting layer for custom workflow logic
- a widget/plugin system for custom UI extensions
- an optional `Java` integration bridge for enterprise connectors

This is a Qntrl-aligned implementation strategy based on publicly documented capabilities, not a claim that we know Qntrl's full private core stack.

## Sources Used For Stack Direction

- Microsoft .NET support policy: `.NET 10` is current `LTS` and supported until `November 14, 2028`
- React official docs
- Next.js official docs
- PostgreSQL official docs
- Vite official docs
- React Flow official docs
- Qntrl client scripts docs
- Qntrl CodeX docs
- Qntrl script modules docs
- Qntrl widgets docs
- Qntrl bridge docs

## Next Recommended Action

The first file you should create after this README is:

- `docs/prd.md`

Then create:

- `docs/domain-model.md`
- `docs/workflow-schema.md`

After that, start the backend and frontend skeleton.
