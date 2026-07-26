namespace HermesZoneTorba.Domain.Common;

/// <summary>
/// RAM/VRAM/disk footprint, used both as a model's declared requirement (Model Registry) and as a
/// running instance's configured ceiling (Hermes Manager). See docs/07-local-llm.md.
/// </summary>
public sealed class ResourceRequirement : ValueObject
{
    public long MinimumRamBytes { get; private set; }
    public long RecommendedRamBytes { get; private set; }
    public long? MinimumVramBytes { get; private set; }
    public long DiskBytes { get; private set; }

    // EF Core materialization constructor for this owned type — see docs/12-database.md#conventions.
    private ResourceRequirement()
    {
    }

    public ResourceRequirement(long minimumRamBytes, long recommendedRamBytes, long? minimumVramBytes, long diskBytes)
    {
        if (minimumRamBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumRamBytes), "Minimum RAM must be positive.");
        if (recommendedRamBytes < minimumRamBytes)
            throw new ArgumentOutOfRangeException(nameof(recommendedRamBytes), "Recommended RAM cannot be below the minimum.");
        if (diskBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(diskBytes), "Disk requirement cannot be negative.");

        MinimumRamBytes = minimumRamBytes;
        RecommendedRamBytes = recommendedRamBytes;
        MinimumVramBytes = minimumVramBytes;
        DiskBytes = diskBytes;
    }

    /// <summary>True when a hardware profile's available resources meet the minimum (not recommended) bar.</summary>
    public bool FitsWithin(long availableRamBytes, long? availableVramBytes) =>
        availableRamBytes >= MinimumRamBytes &&
        (MinimumVramBytes is null || (availableVramBytes ?? 0) >= MinimumVramBytes);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MinimumRamBytes;
        yield return RecommendedRamBytes;
        yield return MinimumVramBytes;
        yield return DiskBytes;
    }
}
