using System.Text.RegularExpressions;

namespace HermesZoneTorba.Domain.Common;

/// <summary>Semantic version, used for Hermes releases, model versions, and plugin versions.</summary>
public sealed partial class SemVer : ValueObject, IComparable<SemVer>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? PreRelease { get; }

    private SemVer(int major, int minor, int patch, string? preRelease)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
    }

    public static SemVer Parse(string value)
    {
        var match = VersionPattern().Match(value);
        if (!match.Success)
            throw new FormatException($"'{value}' is not a valid semantic version.");

        return new SemVer(
            int.Parse(match.Groups["major"].Value),
            int.Parse(match.Groups["minor"].Value),
            int.Parse(match.Groups["patch"].Value),
            match.Groups["pre"].Success ? match.Groups["pre"].Value : null);
    }

    public int CompareTo(SemVer? other)
    {
        if (other is null) return 1;
        var byMajor = Major.CompareTo(other.Major);
        if (byMajor != 0) return byMajor;
        var byMinor = Minor.CompareTo(other.Minor);
        if (byMinor != 0) return byMinor;
        return Patch.CompareTo(other.Patch);
    }

    public override string ToString() => PreRelease is null ? $"{Major}.{Minor}.{Patch}" : $"{Major}.{Minor}.{Patch}-{PreRelease}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Major;
        yield return Minor;
        yield return Patch;
        yield return PreRelease;
    }

    [GeneratedRegex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<pre>[0-9A-Za-z.-]+))?$")]
    private static partial Regex VersionPattern();
}
