using HermesZoneTorba.Application.Common.Interfaces;

namespace HermesZoneTorba.Infrastructure.Common;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
