using System.Security.Claims;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Api.Security;
using Hopaut.BuildingBlocks.Application.Security;
using Microsoft.AspNetCore.Http;

namespace Hopaut.BuildingBlocks.UnitTests.Api;

public sealed class HttpContextCurrentUserTests
{
    [Fact]
    public void GivenAnAuthenticatedPrincipalWithAUserIdClaim_WhenRead_ThenTheIdAndRolesAreExposed()
    {
        // Arrange
        var currentUser = CurrentUserFor(new ClaimsIdentity(
            [new Claim(HopautClaimTypes.UserId, "user-123"), new Claim(ClaimTypes.Role, "Admin")],
            authenticationType: "Bearer"));

        // Act
        var id = currentUser.Id;

        // Assert
        using (new AssertionScope())
        {
            currentUser.IsAuthenticated.Should().BeTrue();
            id.Value.Should().Be("user-123");
            currentUser.IsInRole("Admin").Should().BeTrue();
            currentUser.IsInRole("SuperAdmin").Should().BeFalse();
        }
    }

    [Fact]
    public void GivenAnAnonymousRequest_WhenTheIdIsRead_ThenItThrowsInsteadOfFallingBackToAnEmptyId()
    {
        // Arrange
        var currentUser = CurrentUserFor(new ClaimsIdentity([new Claim(HopautClaimTypes.UserId, "user-123")]));

        // Act
        var act = () => currentUser.Id;

        // Assert
        using (new AssertionScope())
        {
            currentUser.IsAuthenticated.Should().BeFalse();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    [Fact]
    public void GivenAnAuthenticatedPrincipalWithoutAUserIdClaim_WhenInspected_ThenItIsNotTreatedAsAuthenticated()
    {
        // Arrange
        var currentUser = CurrentUserFor(new ClaimsIdentity([new Claim(ClaimTypes.Email, "alice@hopaut.app")], "Bearer"));

        // Act
        var authenticated = currentUser.IsAuthenticated;

        // Assert
        authenticated.Should().BeFalse();
    }

    private static HttpContextCurrentUser CurrentUserFor(ClaimsIdentity identity) =>
        new(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } });
}
