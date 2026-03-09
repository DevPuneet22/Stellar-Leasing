using System.Text.RegularExpressions;
using StellarLeasing.Domain.Common;

namespace StellarLeasing.Domain.WorkflowDefinitions;

public sealed class WorkflowDefinition : Entity
{
    private static readonly Regex CollapseDashesRegex = new("-{2,}", RegexOptions.Compiled);
    private readonly List<WorkflowVersion> _versions = [];

    private WorkflowDefinition(){ } // EF core
    private WorkflowDefinition(Guid tenantId, string name, string code)
    {
        TenantId = tenantId;
        Name = name;
        Code = code;
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Code { get; private set; } = null!;

    public IReadOnlyCollection<WorkflowVersion> Versions => _versions.AsReadOnly();

    public WorkflowVersion? ActiveVersion => _versions.SingleOrDefault(version => version.Status == WorkflowVersionStatus.Active);

    public static WorkflowDefinition Create(Guid tenantId, string name, string? code = null)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Workflow name is required.", nameof(name));
        }

        var normalizedCode = NormalizeCode(code, normalizedName);
        var definition = new WorkflowDefinition(tenantId, normalizedName, normalizedCode);
        definition.AddDraftVersion();
        return definition;
    }

    public WorkflowVersion AddDraftVersion()
    {
        var nextVersionNumber = _versions.Count == 0 ? 1 : _versions.Max(version => version.VersionNumber) + 1;
        var version = WorkflowVersion.CreateDraft(nextVersionNumber, BuildStarterSteps(), BuildStarterTransitions());
        _versions.Add(version);
        return version;
    }

    public void ActivateVersion(int versionNumber)
    {
        var targetVersion = _versions.Single(version => version.VersionNumber == versionNumber);

        foreach (var version in _versions.Where(version => version.Status == WorkflowVersionStatus.Active))
        {
            version.Retire();
        }

        targetVersion.Activate();
    }

    private static string NormalizeCode(string? code, string fallbackName)
    {
        var value = string.IsNullOrWhiteSpace(code) ? fallbackName : code;
        var normalized = value.Trim().ToUpperInvariant().Replace(' ', '-');
        normalized = CollapseDashesRegex.Replace(normalized, "-");
        return normalized;
    }

    private static IReadOnlyCollection<WorkflowStep> BuildStarterSteps()
    {
        return
        [
            new WorkflowStep("start", "Start", WorkflowStepType.Start),
            new WorkflowStep("manager-review", "Manager Review", WorkflowStepType.Approval, "role:manager"),
            new WorkflowStep("operations-check", "Operations Check", WorkflowStepType.Task, "team:operations"),
            new WorkflowStep("complete", "Complete", WorkflowStepType.End)
        ];
    }

    private static IReadOnlyCollection<WorkflowTransition> BuildStarterTransitions()
    {
        return
        [
            new WorkflowTransition("start", "manager-review"),
            new WorkflowTransition("manager-review", "operations-check", "approved"),
            new WorkflowTransition("operations-check", "complete")
        ];
    }
}
