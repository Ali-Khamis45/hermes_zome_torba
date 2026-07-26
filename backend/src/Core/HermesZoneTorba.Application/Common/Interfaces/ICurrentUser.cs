namespace HermesZoneTorba.Application.Common.Interfaces;

/// <summary>
/// The authenticated principal for the current request, as seen by the Application layer. Implemented
/// in Infrastructure by reading the ASP.NET Core HttpContext — Application never touches HttpContext
/// directly. Backs the AuthorizationBehavior pipeline step; see docs/11-security.md.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Permissions { get; }

    bool HasPermission(string permission);
}
