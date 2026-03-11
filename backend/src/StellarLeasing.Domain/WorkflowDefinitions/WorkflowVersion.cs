namespace StellarLeasing.Domain.WorkflowDefinitions;

public sealed class WorkflowVersion
{
    private readonly List<WorkflowStep> _steps= [];
    private readonly List<WorkflowTransition> _transitions = [];

    private WorkflowVersion() { } // EF core
    private WorkflowVersion(
        int versionNumber,
        WorkflowVersionStatus status,
        IEnumerable<WorkflowStep> steps,
        IEnumerable<WorkflowTransition> transitions)
    {
        Id = Guid.NewGuid();
        VersionNumber = versionNumber;
        Status = status;
        _steps.AddRange(steps);
        _transitions.AddRange(transitions);
    }

    public Guid Id { get; private set; }
    public int VersionNumber { get; private set; }

    public WorkflowVersionStatus Status { get; private set; }

    public IReadOnlyCollection<WorkflowStep> Steps => _steps.AsReadOnly();

    public IReadOnlyCollection<WorkflowTransition> Transitions => _transitions.AsReadOnly();

    public static WorkflowVersion CreateDraft(
        int versionNumber,
        IEnumerable<WorkflowStep> steps,
        IEnumerable<WorkflowTransition> transitions)
    {
        var stepList = steps.ToList();
        var transitionList = transitions.ToList();
        ValidateGraph(stepList, transitionList);
        return new WorkflowVersion(versionNumber, WorkflowVersionStatus.Draft, stepList, transitionList);
    }

    public void Activate()
    {
        if (Status == WorkflowVersionStatus.Retired)
        {
            throw new InvalidOperationException("Retired workflow versions cannot be reactivated.");
        }

        Status = WorkflowVersionStatus.Active;
    }

    public void Retire()
    {
        Status = WorkflowVersionStatus.Retired;
    }

    public void UpdateDraft(IEnumerable<WorkflowStep> steps, IEnumerable<WorkflowTransition> transitions)
    {
        if (Status != WorkflowVersionStatus.Draft)
        {
            throw new InvalidOperationException("Only draft workflow versions can be updated.");
        }

        var stepList = steps.ToList();
        var transitionList = transitions.ToList();
        ValidateGraph(stepList, transitionList);

        _steps.Clear();
        _steps.AddRange(stepList);

        _transitions.Clear();
        _transitions.AddRange(transitionList);
    }

    public WorkflowVersion CreateNextDraft(int versionNumber)
    {
        return CreateDraft(
            versionNumber,
            _steps
                .OrderBy(step => step.SortOrder)
                .Select((step, index) => step.CopyForDraft(index)),
            _transitions
                .OrderBy(transition => transition.SortOrder)
                .Select((transition, index) => transition.CopyForDraft(index)));
    }

    private static void ValidateGraph(
        IReadOnlyCollection<WorkflowStep> steps,
        IReadOnlyCollection<WorkflowTransition> transitions)
    {
        if (steps.Count == 0)
        {
            throw new ArgumentException("At least one workflow step is required.", nameof(steps));
        }

        if (transitions.Count == 0)
        {
            throw new ArgumentException("At least one workflow transition is required.", nameof(transitions));
        }

        var duplicateKey = steps
            .GroupBy(step => step.Key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateKey is not null)
        {
            throw new ArgumentException($"Workflow step key '{duplicateKey.Key}' is duplicated.", nameof(steps));
        }

        var startCount = steps.Count(step => step.StepType == WorkflowStepType.Start);
        if (startCount != 1)
        {
            throw new ArgumentException("A workflow must contain exactly one start step.", nameof(steps));
        }

        var endCount = steps.Count(step => step.StepType == WorkflowStepType.End);
        if (endCount == 0)
        {
            throw new ArgumentException("A workflow must contain at least one end step.", nameof(steps));
        }

        var stepKeys = steps
            .Select(step => step.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var startStep = steps.Single(step => step.StepType == WorkflowStepType.Start);
        var endStepKeys = steps
            .Where(step => step.StepType == WorkflowStepType.End)
            .Select(step => step.Key)
            .ToArray();
        var outgoingTransitions = stepKeys.ToDictionary(
            stepKey => stepKey,
            _ => new List<string>(),
            StringComparer.OrdinalIgnoreCase);
        var incomingTransitions = stepKeys.ToDictionary(
            stepKey => stepKey,
            _ => new List<string>(),
            StringComparer.OrdinalIgnoreCase);
        var transitionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var transition in transitions)
        {
            if (!stepKeys.Contains(transition.FromStepKey))
            {
                throw new ArgumentException(
                    $"Transition source step '{transition.FromStepKey}' does not exist.",
                    nameof(transitions));
            }

            if (!stepKeys.Contains(transition.ToStepKey))
            {
                throw new ArgumentException(
                    $"Transition target step '{transition.ToStepKey}' does not exist.",
                    nameof(transitions));
            }

            if (string.Equals(transition.FromStepKey, transition.ToStepKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Workflow transition '{transition.FromStepKey}' cannot point to itself.",
                    nameof(transitions));
            }

            if (string.Equals(transition.ToStepKey, startStep.Key, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The start step cannot receive incoming transitions.", nameof(transitions));
            }

            if (endStepKeys.Contains(transition.FromStepKey, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("End steps cannot have outgoing transitions.", nameof(transitions));
            }

            var transitionKey =
                $"{transition.FromStepKey}|{transition.ToStepKey}|{transition.ConditionName ?? string.Empty}";

            if (!transitionKeys.Add(transitionKey))
            {
                throw new ArgumentException(
                    $"Workflow transition '{transition.FromStepKey}' -> '{transition.ToStepKey}' is duplicated.",
                    nameof(transitions));
            }

            outgoingTransitions[transition.FromStepKey].Add(transition.ToStepKey);
            incomingTransitions[transition.ToStepKey].Add(transition.FromStepKey);
        }

        var reachableFromStart = Traverse(startStep.Key, outgoingTransitions);
        var unreachableSteps = steps
            .Select(step => step.Key)
            .Where(stepKey => !reachableFromStart.Contains(stepKey))
            .ToArray();

        if (unreachableSteps.Length > 0)
        {
            throw new ArgumentException(
                $"Every step must be reachable from the start step. Unreachable: {string.Join(", ", unreachableSteps)}.",
                nameof(transitions));
        }

        var endReachableSteps = endStepKeys
            .SelectMany(endStepKey => Traverse(endStepKey, incomingTransitions))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var deadEndSteps = steps
            .Select(step => step.Key)
            .Where(stepKey => !endReachableSteps.Contains(stepKey))
            .ToArray();

        if (deadEndSteps.Length > 0)
        {
            throw new ArgumentException(
                $"Every step must be able to reach an end step. Dead ends: {string.Join(", ", deadEndSteps)}.",
                nameof(transitions));
        }
    }

    private static HashSet<string> Traverse(
        string origin,
        IReadOnlyDictionary<string, List<string>> adjacency)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>();

        visited.Add(origin);
        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var next in adjacency[current])
            {
                if (!visited.Add(next))
                {
                    continue;
                }

                queue.Enqueue(next);
            }
        }

        return visited;
    }
}
