using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HermesZoneTorba.Architecture.Tests;

/// <summary>
/// Mechanically enforces the layering rules from docs/01-system-architecture.md#layering and
/// docs/15-coding-standards.md#dependency-rules — CI fails the build on violation, layering is not a
/// code-review-only convention. Referenced assemblies are loaded via a type from each project so this
/// test project doesn't need runtime references beyond the ProjectReferences already wired in the .sln.
/// </summary>
public sealed class LayeringTests
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Common.Entity<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_Should_Not_DependOn_ApplicationInfrastructureOrApi()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "HermesZoneTorba.Application",
                "HermesZoneTorba.Infrastructure",
                "HermesZoneTorba.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Application_Should_Not_DependOn_InfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("HermesZoneTorba.Infrastructure", "HermesZoneTorba.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_DependOn_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("HermesZoneTorba.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void CommandHandlers_Should_HaveNameEndingWith_Handler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void DomainAggregates_Should_NotHavePublicSetters_OnStatusLikeProperties()
    {
        // Spot-checks the "behavior methods, not public setters" rule from
        // docs/15-coding-standards.md#ddd-conventions against the reference aggregate.
        var statusProperty = typeof(Domain.HermesManagement.HermesInstance).GetProperty(nameof(Domain.HermesManagement.HermesInstance.Status));

        statusProperty.Should().NotBeNull();
        statusProperty!.SetMethod.Should().NotBeNull();
        statusProperty.SetMethod!.IsPublic.Should().BeFalse(
            "aggregate state must change only through behavior methods, never a public setter");
    }

    private static string BuildFailureMessage(TestResult result) =>
        result.FailingTypes is null
            ? "Architecture rule violated."
            : "Architecture rule violated by: " + string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
