namespace StellarLeasing.Application.WorkflowDefinitions;

public sealed record ActivateWorkflowVersionRequest(int VersionNumber, int ExpectedRevision);
