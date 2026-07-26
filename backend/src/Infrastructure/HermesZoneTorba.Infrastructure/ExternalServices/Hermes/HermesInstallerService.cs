using HermesZoneTorba.Application.HermesManagement;
using HermesZoneTorba.Domain.Common;
using Microsoft.Extensions.Logging;

namespace HermesZoneTorba.Infrastructure.ExternalServices.Hermes;

/// <summary>
/// Reference implementation of the download/verify/extract flow described in
/// docs/06-hermes-integration.md#install-flow. This ships as a working stub against a local release
/// manifest so the vertical slice runs end to end without a live Hermes release server configured —
/// swap <see cref="ResolveManifestAsync"/> and <see cref="DownloadAndVerifyAsync"/> for real HTTP calls
/// once the release channel exists.
/// </summary>
public sealed class HermesInstallerService(ILogger<HermesInstallerService> logger) : IHermesInstallerService
{
    private static readonly SemVer LatestKnownVersion = SemVer.Parse("1.0.0");

    public async Task<SemVer> InstallAsync(SemVer? requestedVersion, string installPath, CancellationToken cancellationToken = default)
    {
        var resolvedVersion = requestedVersion ?? await ResolveManifestAsync(cancellationToken);

        logger.LogInformation("Installing Hermes {Version} to {InstallPath}", resolvedVersion, installPath);

        await DownloadAndVerifyAsync(resolvedVersion, installPath, cancellationToken);

        Directory.CreateDirectory(installPath);

        logger.LogInformation("Hermes {Version} installed to {InstallPath}", resolvedVersion, installPath);

        return resolvedVersion;
    }

    private static Task<SemVer> ResolveManifestAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // TODO(Phase 1): fetch the signed version manifest from the configured release channel
        // instead of a hardcoded constant — see docs/06-hermes-integration.md#version-management.
        return Task.FromResult(LatestKnownVersion);
    }

    private static Task DownloadAndVerifyAsync(SemVer version, string installPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // TODO(Phase 1): download the release artifact, verify its checksum + signature per
        // docs/11-security.md, and extract atomically (blue/green swap) per
        // docs/06-hermes-integration.md#update-flow.
        _ = version;
        _ = installPath;
        return Task.CompletedTask;
    }
}
