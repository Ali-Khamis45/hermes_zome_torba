using HermesZoneTorba.Domain.Common;

namespace HermesZoneTorba.Domain.HermesManagement;

/// <summary>
/// Aggregate root for a single managed Hermes agent process. Owns install/configure/start/stop/restart/
/// repair lifecycle and the invariants around them — see docs/03-domain-model.md#hermes-management and
/// docs/06-hermes-integration.md.
/// </summary>
public sealed class HermesInstance : AggregateRoot<Guid>
{
    public SemVer? Version { get; private set; }
    public InstanceStatus Status { get; private set; } = InstanceStatus.NotInstalled;
    public HermesConfiguration? Configuration { get; private set; }
    public string InstallPath { get; private set; } = string.Empty;
    public DateTimeOffset? InstalledAt { get; private set; }
    public DateTimeOffset? LastHealthCheckAt { get; private set; }
    public string? LastFaultReason { get; private set; }

    private HermesInstance() { } // EF Core

    private HermesInstance(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Begins installation. The instance exists in the store from this point (Status: Installing).
    /// <paramref name="requestedVersion"/> is null when the caller asked for "latest" — the actual
    /// resolved version isn't known until the installer completes, see CompleteInstall.
    /// </summary>
    public static HermesInstance BeginInstall(Guid id, SemVer? requestedVersion, string installPath)
    {
        if (string.IsNullOrWhiteSpace(installPath))
            throw new ArgumentException("An install path is required.", nameof(installPath));

        var instance = new HermesInstance(id)
        {
            InstallPath = installPath,
            Status = InstanceStatus.Installing
        };

        instance.Raise(new HermesInstallStarted(id, requestedVersion, DateTimeOffset.UtcNow));
        return instance;
    }

    public void CompleteInstall(SemVer installedVersion)
    {
        if (Status != InstanceStatus.Installing)
            throw new InvalidHermesStateTransitionException(Status, nameof(CompleteInstall));

        Version = installedVersion ?? throw new ArgumentNullException(nameof(installedVersion));
        Status = InstanceStatus.Stopped;
        InstalledAt = DateTimeOffset.UtcNow;
        Raise(new HermesInstallCompleted(Id, installedVersion, DateTimeOffset.UtcNow));
    }

    public void FailInstall(string reason)
    {
        if (Status != InstanceStatus.Installing)
            throw new InvalidHermesStateTransitionException(Status, nameof(FailInstall));

        Status = InstanceStatus.Faulted;
        LastFaultReason = reason;
        Raise(new HermesInstallFailed(Id, reason, DateTimeOffset.UtcNow));
    }

    public void Configure(HermesConfiguration configuration)
    {
        if (Status is InstanceStatus.Installing or InstanceStatus.Uninstalling)
            throw new InvalidHermesStateTransitionException(Status, nameof(Configure));

        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Raise(new HermesConfigurationChanged(Id, DateTimeOffset.UtcNow));
    }

    public void Start()
    {
        if (Status is not (InstanceStatus.Stopped or InstanceStatus.Faulted))
            throw new InvalidHermesStateTransitionException(Status, nameof(Start));
        if (Configuration is null)
            throw new HermesConfigurationRequiredException();

        Status = InstanceStatus.Running;
        LastFaultReason = null;
        Raise(new HermesStarted(Id, DateTimeOffset.UtcNow));
    }

    public void Stop(string reason = "Requested")
    {
        if (Status is not (InstanceStatus.Running or InstanceStatus.Starting))
        {
            if (Status == InstanceStatus.Stopped)
                return; // idempotent

            throw new InvalidHermesStateTransitionException(Status, nameof(Stop));
        }

        Status = InstanceStatus.Stopped;
        Raise(new HermesStopped(Id, reason, DateTimeOffset.UtcNow));
    }

    /// <summary>Always goes through Stop() then Start() — see docs/06-hermes-integration.md.</summary>
    public void Restart(string reason)
    {
        Stop(reason);
        Start();
    }

    public void RecordHealthCheck() => LastHealthCheckAt = DateTimeOffset.UtcNow;

    public void MarkFaulted(string reason)
    {
        Status = InstanceStatus.Faulted;
        LastFaultReason = reason;
        Raise(new HermesFaulted(Id, reason, DateTimeOffset.UtcNow));
    }

    public void CompleteRepair(string summary)
    {
        if (Status != InstanceStatus.Repairing)
            throw new InvalidHermesStateTransitionException(Status, nameof(CompleteRepair));

        Status = InstanceStatus.Stopped;
        LastFaultReason = null;
        Raise(new HermesRepaired(Id, summary, DateTimeOffset.UtcNow));
    }

    public void BeginRepair()
    {
        if (Status != InstanceStatus.Faulted)
            throw new InvalidHermesStateTransitionException(Status, nameof(BeginRepair));

        Status = InstanceStatus.Repairing;
    }
}
