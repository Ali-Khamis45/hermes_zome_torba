using HermesZoneTorba.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HermesZoneTorba.Application.HermesManagement.Queries.GetHermesStatus;

public sealed class GetHermesStatusQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetHermesStatusQuery, HermesStatusDto?>
{
    public async Task<HermesStatusDto?> Handle(GetHermesStatusQuery request, CancellationToken cancellationToken)
    {
        var instance = await dbContext.HermesInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InstanceId, cancellationToken);

        return instance is null
            ? null
            : new HermesStatusDto(
                instance.Id,
                instance.Version?.ToString(),
                instance.Status.ToString(),
                instance.InstalledAt,
                instance.LastHealthCheckAt,
                instance.LastFaultReason);
    }
}
