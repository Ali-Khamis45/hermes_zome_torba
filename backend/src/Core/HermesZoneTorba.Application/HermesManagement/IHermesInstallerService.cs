using HermesZoneTorba.Domain.Common;

namespace HermesZoneTorba.Application.HermesManagement;

/// <summary>
/// Download/verify/extract for a Hermes release. Defined here (Application) and implemented in
/// Infrastructure — see docs/06-hermes-integration.md#install-flow and
/// docs/01-system-architecture.md#layering for why the dependency points this direction.
/// </summary>
public interface IHermesInstallerService
{
    /// <summary>
    /// Resolves the requested (or latest compatible) version, downloads and verifies the release
    /// artifact, and extracts it to <paramref name="installPath"/>. Throws on integrity failure —
    /// callers are expected to translate that into HermesInstance.FailInstall.
    /// </summary>
    Task<SemVer> InstallAsync(SemVer? requestedVersion, string installPath, CancellationToken cancellationToken = default);
}
