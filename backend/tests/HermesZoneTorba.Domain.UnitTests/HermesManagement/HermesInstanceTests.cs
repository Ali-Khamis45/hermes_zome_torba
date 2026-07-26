using FluentAssertions;
using HermesZoneTorba.Domain.Common;
using HermesZoneTorba.Domain.HermesManagement;
using Xunit;

namespace HermesZoneTorba.Domain.UnitTests.HermesManagement;

/// <summary>
/// Exercises the invariants described in docs/03-domain-model.md#hermes-management directly against the
/// aggregate's public methods — no reflection-based shortcuts, per docs/16-testing.md.
/// </summary>
public sealed class HermesInstanceTests
{
    private static HermesConfiguration ValidConfiguration() => new(
        provider: ModelProvider.Ollama,
        defaultModel: "llama3.1:8b",
        maxConcurrentTasks: 4,
        resourceLimits: new ResourceRequirement(
            minimumRamBytes: 8L * 1024 * 1024 * 1024,
            recommendedRamBytes: 16L * 1024 * 1024 * 1024,
            minimumVramBytes: null,
            diskBytes: 5L * 1024 * 1024 * 1024),
        toolsEnabled: ["terminal-agent", "file-agent"],
        workingDirectory: "/var/hzt/instances/instance-1");

    [Fact]
    public void BeginInstall_SetsStatusToInstalling_AndRaisesHermesInstallStarted()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), SemVer.Parse("1.0.0"), "/opt/hermes");

        instance.Status.Should().Be(InstanceStatus.Installing);
        instance.DomainEvents.Should().ContainSingle(e => e is HermesInstallStarted);
    }

    [Fact]
    public void CompleteInstall_WhenInstalling_TransitionsToStopped_AndSetsVersion()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");

        instance.CompleteInstall(SemVer.Parse("1.2.3"));

        instance.Status.Should().Be(InstanceStatus.Stopped);
        instance.Version.Should().NotBeNull();
        instance.Version!.ToString().Should().Be("1.2.3");
        instance.InstalledAt.Should().NotBeNull();
        instance.DomainEvents.Should().ContainSingle(e => e is HermesInstallCompleted);
    }

    [Fact]
    public void CompleteInstall_WhenNotInstalling_ThrowsInvalidStateTransition()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");
        instance.CompleteInstall(SemVer.Parse("1.0.0"));

        var act = () => instance.CompleteInstall(SemVer.Parse("1.0.1"));

        act.Should().Throw<InvalidHermesStateTransitionException>();
    }

    [Fact]
    public void Start_WithoutConfiguration_ThrowsConfigurationRequired()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");
        instance.CompleteInstall(SemVer.Parse("1.0.0"));

        var act = instance.Start;

        act.Should().Throw<HermesConfigurationRequiredException>();
    }

    [Fact]
    public void Start_WhenConfiguredAndStopped_TransitionsToRunning()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");
        instance.CompleteInstall(SemVer.Parse("1.0.0"));
        instance.Configure(ValidConfiguration());

        instance.Start();

        instance.Status.Should().Be(InstanceStatus.Running);
        instance.DomainEvents.Should().ContainSingle(e => e is HermesStarted);
    }

    [Fact]
    public void Stop_WhenAlreadyStopped_IsIdempotent_AndDoesNotRaiseAnEvent()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");
        instance.CompleteInstall(SemVer.Parse("1.0.0"));
        instance.ClearDomainEvents();

        instance.Stop();

        instance.Status.Should().Be(InstanceStatus.Stopped);
        instance.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Restart_StopsThenStarts()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");
        instance.CompleteInstall(SemVer.Parse("1.0.0"));
        instance.Configure(ValidConfiguration());
        instance.Start();
        instance.ClearDomainEvents();

        instance.Restart("Configuration changed");

        instance.Status.Should().Be(InstanceStatus.Running);
        instance.DomainEvents.Should().Contain(e => e is HermesStopped).And.Contain(e => e is HermesStarted);
    }

    [Fact]
    public void MarkFaulted_FromRunning_TransitionsToFaulted_AndRecordsReason()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");
        instance.CompleteInstall(SemVer.Parse("1.0.0"));
        instance.Configure(ValidConfiguration());
        instance.Start();

        instance.MarkFaulted("Heartbeat timeout after 30s");

        instance.Status.Should().Be(InstanceStatus.Faulted);
        instance.LastFaultReason.Should().Be("Heartbeat timeout after 30s");
        instance.DomainEvents.Should().Contain(e => e is HermesFaulted);
    }

    [Fact]
    public void BeginRepair_ThenCompleteRepair_ReturnsToStopped_AndClearsFaultReason()
    {
        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), null, "/opt/hermes");
        instance.CompleteInstall(SemVer.Parse("1.0.0"));
        instance.Configure(ValidConfiguration());
        instance.Start();
        instance.MarkFaulted("Deadlock detected");

        instance.BeginRepair();
        instance.CompleteRepair("Cleared stale lock file");

        instance.Status.Should().Be(InstanceStatus.Stopped);
        instance.LastFaultReason.Should().BeNull();
        instance.DomainEvents.Should().Contain(e => e is HermesRepaired);
    }
}
