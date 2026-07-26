using HermesZoneTorba.Application.Common.Interfaces;
using HermesZoneTorba.Domain.Common;
using HermesZoneTorba.Domain.HermesManagement;
using MediatR;

namespace HermesZoneTorba.Application.HermesManagement.Commands.InstallHermes;

/// <summary>
/// Orchestrates the install flow described in docs/06-hermes-integration.md#install-flow: create the
/// aggregate in Installing state, delegate the actual download/verify/extract to the installer service,
/// then transition the aggregate based on the outcome. A failed install is recorded, not thrown away —
/// see docs/06-hermes-integration.md for why partial installs still need a queryable state.
/// </summary>
public sealed class InstallHermesCommandHandler(
    IApplicationDbContext dbContext,
    IHermesInstallerService installerService)
    : IRequestHandler<InstallHermesCommand, InstallHermesResult>
{
    public async Task<InstallHermesResult> Handle(InstallHermesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestedVersion = string.IsNullOrWhiteSpace(request.RequestedVersion)
            ? null
            : SemVer.Parse(request.RequestedVersion);

        var instance = HermesInstance.BeginInstall(Guid.NewGuid(), requestedVersion, request.InstallPath);
        dbContext.HermesInstances.Add(instance);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var installedVersion = await installerService.InstallAsync(requestedVersion, request.InstallPath, cancellationToken);
            instance.CompleteInstall(installedVersion);
        }
        catch (Exception ex)
        {
            instance.FailInstall(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new InstallHermesResult(instance.Id, instance.Version?.ToString() ?? "unknown", instance.Status.ToString());
    }
}
