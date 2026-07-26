using FluentAssertions;
using HermesZoneTorba.Application.HermesManagement;
using HermesZoneTorba.Application.HermesManagement.Commands.InstallHermes;
using HermesZoneTorba.Domain.Common;
using HermesZoneTorba.Domain.HermesManagement;
using NSubstitute;
using Xunit;

namespace HermesZoneTorba.Application.UnitTests.HermesManagement;

/// <summary>
/// Exercises the orchestration in docs/06-hermes-integration.md#install-flow: successful installs
/// complete the aggregate, failed installs are recorded as Faulted rather than lost.
/// </summary>
public sealed class InstallHermesCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenInstallerSucceeds_PersistsCompletedInstance()
    {
        await using var dbContext = TestDbContext.CreateInMemory();
        var installer = Substitute.For<IHermesInstallerService>();
        installer.InstallAsync(Arg.Any<SemVer?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(SemVer.Parse("1.0.0"));

        var handler = new InstallHermesCommandHandler(dbContext, installer);
        var command = new InstallHermesCommand(RequestedVersion: null, InstallPath: "/opt/hermes");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(InstanceStatus.Stopped));
        result.Version.Should().Be("1.0.0");

        var persisted = await dbContext.HermesInstances.FindAsync(result.InstanceId);
        persisted!.Status.Should().Be(InstanceStatus.Stopped);
    }

    [Fact]
    public async Task Handle_WhenInstallerThrows_PersistsFaultedInstance_InsteadOfPropagating()
    {
        await using var dbContext = TestDbContext.CreateInMemory();
        var installer = Substitute.For<IHermesInstallerService>();
        installer.InstallAsync(Arg.Any<SemVer?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<SemVer>(_ => throw new InvalidOperationException("Checksum mismatch"));

        var handler = new InstallHermesCommandHandler(dbContext, installer);
        var command = new InstallHermesCommand(RequestedVersion: "1.0.0", InstallPath: "/opt/hermes");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(InstanceStatus.Faulted));

        var persisted = await dbContext.HermesInstances.FindAsync(result.InstanceId);
        persisted!.Status.Should().Be(InstanceStatus.Faulted);
        persisted.LastFaultReason.Should().Be("Checksum mismatch");
    }
}
