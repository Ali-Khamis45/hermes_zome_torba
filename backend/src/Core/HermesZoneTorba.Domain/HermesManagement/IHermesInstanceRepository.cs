using HermesZoneTorba.Domain.Common;

namespace HermesZoneTorba.Domain.HermesManagement;

public interface IHermesInstanceRepository : IRepository<HermesInstance, Guid>
{
    Task<IReadOnlyList<HermesInstance>> ListAsync(CancellationToken cancellationToken = default);
}
