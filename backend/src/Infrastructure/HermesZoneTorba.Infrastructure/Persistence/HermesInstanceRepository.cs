using HermesZoneTorba.Domain.HermesManagement;
using Microsoft.EntityFrameworkCore;

namespace HermesZoneTorba.Infrastructure.Persistence;

public sealed class HermesInstanceRepository(ApplicationDbContext dbContext) : IHermesInstanceRepository
{
    public Task<HermesInstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.HermesInstances.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<HermesInstance>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.HermesInstances.AsNoTracking().ToListAsync(cancellationToken);

    public void Add(HermesInstance aggregate) => dbContext.HermesInstances.Add(aggregate);

    public void Update(HermesInstance aggregate) => dbContext.HermesInstances.Update(aggregate);

    public void Remove(HermesInstance aggregate) => dbContext.HermesInstances.Remove(aggregate);
}
