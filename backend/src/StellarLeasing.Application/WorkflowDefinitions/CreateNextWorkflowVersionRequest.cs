namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record CreateNextWorkflowVersionRequest(int ExpectedRevision);
