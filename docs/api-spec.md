# API Starter Spec

## System

- `GET /api/system/health`
- `GET /api/system/info`

## Workflow Definitions

- `GET /api/workflow-definitions`
- `POST /api/workflow-definitions`
- `GET /api/workflow-definitions/{id}` later
- `POST /api/workflow-definitions/{id}/versions` later
- `POST /api/workflow-definitions/{id}/activate` later

## Runtime

- `POST /api/process-instances` later
- `GET /api/process-instances/{id}` later
- `POST /api/tasks/{id}/approve` later
- `POST /api/tasks/{id}/reject` later

## Auth

- `POST /api/auth/login` later
- `POST /api/auth/refresh` later
