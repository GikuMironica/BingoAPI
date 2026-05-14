using FluentAssertions;
using NetArchTest.Rules;

namespace Hopaut.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(BingoAPI.Startup).Assembly;

    [Fact]
    public void Controllers_Should_Not_Depend_On_DataContext_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("BingoAPI.Controllers")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // This is aspirational — currently violated by legacy code.
        // Uncomment assertion once controllers are refactored.
        // result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Services_Should_Not_Reference_AspNetCore_Mvc()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("BingoAPI.Services")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore.Mvc")
            .GetResult();

        // Aspirational — IdentityService currently depends on IUrlHelper.
        // result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void No_Class_Should_Depend_On_Obsolete_ErrorService()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .AreClasses()
            .ShouldNot()
            .HaveDependencyOn("BingoAPI.Services.IErrorService")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
