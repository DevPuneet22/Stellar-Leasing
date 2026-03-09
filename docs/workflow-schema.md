# Workflow Schema

## Design Principles

- the visual builder and form-based editor must write the same schema
- runtime execution should read a versioned snapshot, not a mutable draft
- each step should have stable keys to support audit and migration safely

## Starter JSON Shape

```json
{
  "name": "Lease Approval",
  "code": "LEASE-APPROVAL",
  "version": 1,
  "trigger": {
    "type": "manual"
  },
  "steps": [
    {
      "key": "start",
      "name": "Start",
      "type": "start"
    },
    {
      "key": "manager-review",
      "name": "Manager Review",
      "type": "approval",
      "assigneeRule": "role:manager"
    },
    {
      "key": "operations-check",
      "name": "Operations Check",
      "type": "task",
      "assigneeRule": "team:operations"
    },
    {
      "key": "complete",
      "name": "Complete",
      "type": "end"
    }
  ],
  "transitions": [
    {
      "from": "start",
      "to": "manager-review"
    },
    {
      "from": "manager-review",
      "to": "operations-check",
      "condition": "approved"
    },
    {
      "from": "operations-check",
      "to": "complete"
    }
  ]
}
```

## Required Runtime Fields Later

- due date rule
- escalation rule
- form mapping
- SLA policy
- notification events
- visibility rules
