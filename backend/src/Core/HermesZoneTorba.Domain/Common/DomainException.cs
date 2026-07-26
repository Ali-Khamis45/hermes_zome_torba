namespace HermesZoneTorba.Domain.Common;

/// <summary>
/// Base type for invariant violations raised from within an aggregate. Mapped to RFC 9457 Problem
/// Details by HermesZoneTorba.Api's exception-handling middleware — see docs/04-api-spec.md.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
