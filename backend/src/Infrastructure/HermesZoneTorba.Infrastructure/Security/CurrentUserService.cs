using System.Security.Claims;
using HermesZoneTorba.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HermesZoneTorba.Infrastructure.Security;

/// <summary>
/// Reads the authenticated principal from the current HTTP request. Application depends on
/// ICurrentUser only — this is the one place that touches HttpContext directly, see
/// docs/01-system-architecture.md#layering.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var subject = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(subject, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Permissions =>
        httpContextAccessor.HttpContext?.User.FindAll("permission").Select(c => c.Value).ToArray() ?? [];

    public bool HasPermission(string permission) => Permissions.Contains(permission, StringComparer.Ordinal);
}
