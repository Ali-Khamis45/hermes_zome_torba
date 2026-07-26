using FluentAssertions;
using HermesZoneTorba.Domain.Common;
using Xunit;

namespace HermesZoneTorba.Domain.UnitTests.Common;

public sealed class SemVerTests
{
    [Theory]
    [InlineData("1.0.0")]
    [InlineData("2.14.7")]
    [InlineData("1.0.0-rc.1")]
    public void Parse_AcceptsValidVersions(string value)
    {
        var act = () => SemVer.Parse(value);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("not-a-version")]
    [InlineData("")]
    public void Parse_RejectsInvalidVersions(string value)
    {
        var act = () => SemVer.Parse(value);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void CompareTo_OrdersByMajorThenMinorThenPatch()
    {
        SemVer.Parse("2.0.0").CompareTo(SemVer.Parse("1.9.9")).Should().BePositive();
        SemVer.Parse("1.2.0").CompareTo(SemVer.Parse("1.10.0")).Should().BeNegative();
        SemVer.Parse("1.2.3").CompareTo(SemVer.Parse("1.2.3")).Should().Be(0);
    }

    [Fact]
    public void SameVersionValues_AreEqual_AsValueObjects()
    {
        SemVer.Parse("1.4.0").Should().Be(SemVer.Parse("1.4.0"));
    }
}
