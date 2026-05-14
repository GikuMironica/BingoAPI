using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace Hopaut.ArchitectureTests;

/// <summary>
/// Architecture tests enforcing DDD constraints on modular monolith building blocks.
/// These tests ensure proper layering and aggregate design conventions.
/// </summary>
public class DddEnforcementTests
{
    private static readonly Assembly SharedKernelAssembly =
        typeof(Hopaut.SharedKernel.Entity<>).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(Hopaut.BuildingBlocks.Application.Caching.ICacheService).Assembly;

    [Fact]
    public void SharedKernel_Should_Not_Depend_On_EfCore()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "SharedKernel is a pure domain layer and must not reference EF Core");
    }

    [Fact]
    public void SharedKernel_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.AspNetCore.Mvc",
                "Microsoft.AspNetCore.Http",
                "Microsoft.AspNetCore.Routing")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "SharedKernel must not reference ASP.NET Core");
    }

    [Fact]
    public void SharedKernel_Should_Not_Depend_On_Hangfire()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn("Hangfire")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "SharedKernel must not reference Hangfire");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.AspNetCore.Mvc",
                "Microsoft.AspNetCore.Http",
                "Microsoft.AspNetCore.Routing")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application layer must not reference ASP.NET Core");
    }

    [Fact]
    public void AggregateRoots_Should_Have_Private_Constructors()
    {
        // Find all concrete types that inherit from AggregateRoot<>
        var aggregateTypes = Types.InAssembly(SharedKernelAssembly)
            .That()
            .Inherit(typeof(Hopaut.SharedKernel.AggregateRoot<>))
            .And()
            .AreNotAbstract()
            .GetTypes();

        foreach (var type in aggregateTypes)
        {
            var publicCtors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            publicCtors.Should().BeEmpty(
                $"Aggregate '{type.Name}' should use private constructors with static factory methods");
        }
    }
}
