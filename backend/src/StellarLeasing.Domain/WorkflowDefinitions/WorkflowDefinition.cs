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
        Revision = 0;
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Code { get; private set; } = null!;

    public int Revision { get; private set; }

    public IReadOnlyCollection<WorkflowVersion> Versions => _versions.AsReadOnly();

    public WorkflowVersion? ActiveVersion => _versions.SingleOrDefault(version => version.Status == WorkflowVersionStatus.Active);

    public WorkflowVersion? DraftVersion => _versions
        .Where(version => version.Status == WorkflowVersionStatus.Draft)
        .OrderByDescending(version => version.VersionNumber)
        .FirstOrDefault();

    public static WorkflowDefinition Create(Guid tenantId, string name, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workflow name is required.", nameof(name));
        }

        var normalizedName = name.Trim();
        var normalizedCode = NormalizeCode(code, normalizedName);
        var definition = new WorkflowDefinition(tenantId, normalizedName, normalizedCode);
        definition.AddDraftVersion();
        return definition;
    }

    public WorkflowVersion AddDraftVersion()
    {
        if (DraftVersion is not null)
        {
            throw new InvalidOperationException("A draft version already exists for this workflow definition.");
        }

        var nextVersionNumber = _versions.Count == 0 ? 1 : _versions.Max(version => version.VersionNumber) + 1;
        var version = _versions.Count == 0
            ? WorkflowVersion.CreateDraft(nextVersionNumber, BuildStarterSteps(), BuildStarterTransitions())
            : _versions
                .OrderByDescending(existingVersion => existingVersion.VersionNumber)
                .First()
                .CreateNextDraft(nextVersionNumber);

        _versions.Add(version);
        Touch();
        return version;
    }

    public void ActivateVersion(int versionNumber)
    {
        var targetVersion = _versions.SingleOrDefault(version => version.VersionNumber == versionNumber)
            ?? throw new InvalidOperationException($"Workflow version '{versionNumber}' was not found.");

        foreach (var version in _versions.Where(version => version.Status == WorkflowVersionStatus.Active))
        {
            version.Retire();
        }

        targetVersion.Activate();
        Touch();
    }

    public void Rename(string name, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workflow name is required.", nameof(name));
        }

        var normalizedName = name.Trim();
        Name = normalizedName;
        Code = NormalizeCode(code, normalizedName);
    }

    public void UpdateDraftVersion(IEnumerable<WorkflowStep> steps, IEnumerable<WorkflowTransition> transitions)
    {
        var draftVersion = DraftVersion
            ?? throw new InvalidOperationException("Create a draft version before editing this workflow.");

        draftVersion.UpdateDraft(steps, transitions);
        Touch();
    }

    private void Touch()
    {
        Revision += 1;
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
            new WorkflowStep("start", "Start", WorkflowStepType.Start, sortOrder: 0, positionX: 80, positionY: 160),
            new WorkflowStep(
                "manager-review",
                "Manager Review",
                WorkflowStepType.Approval,
                "role:manager",
                1,
                360,
                160),
            new WorkflowStep(
                "operations-check",
                "Operations Check",
                WorkflowStepType.Task,
                "team:operations",
                2,
                660,
                160),
            new WorkflowStep("complete", "Complete", WorkflowStepType.End, sortOrder: 3, positionX: 940, positionY: 160)
        ];
    }

    private static IReadOnlyCollection<WorkflowTransition> BuildStarterTransitions()
    {
        return
        [
            new WorkflowTransition("start", "manager-review", sortOrder: 0),
            new WorkflowTransition("manager-review", "operations-check", "approved", 1),
            new WorkflowTransition("operations-check", "complete", sortOrder: 2)
        ];
    }
}
