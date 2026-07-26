using HermesZoneTorba.Domain.Common;

namespace HermesZoneTorba.Domain.HermesManagement;

/// <summary>
/// Immutable configuration for a HermesInstance. Changing configuration replaces the value object
/// wholesale (via HermesInstance.Configure) rather than mutating fields in place — see
/// docs/06-hermes-integration.md#configuration-model.
/// </summary>
public sealed class HermesConfiguration : ValueObject
{
    public ModelProvider Provider { get; private set; }
    public string DefaultModel { get; private set; } = string.Empty;
    public int MaxConcurrentTasks { get; private set; }
    public ResourceRequirement ResourceLimits { get; private set; } = null!;
    public IReadOnlyCollection<string> ToolsEnabled { get; private set; } = [];
    public string WorkingDirectory { get; private set; } = string.Empty;

    // EF Core materialization constructor for this owned type — see docs/12-database.md#conventions.
    private HermesConfiguration()
    {
    }

    public HermesConfiguration(
        ModelProvider provider,
        string defaultModel,
        int maxConcurrentTasks,
        ResourceRequirement resourceLimits,
        IReadOnlyCollection<string> toolsEnabled,
        string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(defaultModel))
            throw new ArgumentException("A default model must be specified.", nameof(defaultModel));
        if (maxConcurrentTasks <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentTasks), "Must allow at least one concurrent task.");
        if (string.IsNullOrWhiteSpace(workingDirectory))
            throw new ArgumentException("A working directory must be specified.", nameof(workingDirectory));

        Provider = provider;
        DefaultModel = defaultModel;
        MaxConcurrentTasks = maxConcurrentTasks;
        ResourceLimits = resourceLimits ?? throw new ArgumentNullException(nameof(resourceLimits));
        ToolsEnabled = toolsEnabled ?? [];
        WorkingDirectory = workingDirectory;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Provider;
        yield return DefaultModel;
        yield return MaxConcurrentTasks;
        yield return ResourceLimits;
        yield return WorkingDirectory;
        foreach (var tool in ToolsEnabled.OrderBy(t => t, StringComparer.Ordinal))
            yield return tool;
    }
}
