using System.Security.Claims;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.Application;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GivenAnAuthenticatedPrincipalWithAUserIdClaim_WhenTheUserIdIsRead_ThenItIsReturned()
    {
        // Arrange
        var principal = Principal(authenticated: true, new Claim(HopautClaimTypes.UserId, "user-123"));

        // Act
        var userId = principal.GetUserId();

        // Assert
        userId.Should().Be(UserId.From("user-123"));
    }

    [Theory]
    [InlineData(false, "user-123")]
    [InlineData(true, "")]
    [InlineData(true, null)]
    public void GivenAnAnonymousPrincipalOrNoValidUserIdClaim_WhenTheUserIdIsRead_ThenItIsNull(bool authenticated, string? claimValue)
    {
        // Arrange
        Claim[] claims = claimValue is null ? [] : [new Claim(HopautClaimTypes.UserId, claimValue)];
        var principal = Principal(authenticated, claims);

        // Act
        var userId = principal.GetUserId();

        // Assert
        userId.Should().BeNull();
    }

    [Theory]
    [InlineData(HopautRoles.Admin, true)]
    [InlineData(HopautRoles.SuperAdmin, true)]
    [InlineData(HopautRoles.User, false)]
    public void GivenAnAuthenticatedPrincipalInARole_WhenAdministratorIsChecked_ThenOnlyAdministratorRolesCount(string role, bool expected)
    {
        // Arrange
        var principal = Principal(authenticated: true, new Claim(ClaimTypes.Role, role));

        // Act
        var isAdministrator = principal.IsAdministrator();

        // Assert
        isAdministrator.Should().Be(expected);
    }

    [Fact]
    public void GivenAnAnonymousPrincipalWithAnAdminRoleClaim_WhenAdministratorIsChecked_ThenItIsNotAnAdministrator()
    {
        // Arrange
        var principal = Principal(authenticated: false, new Claim(ClaimTypes.Role, HopautRoles.Admin));

        // Act
        var isAdministrator = principal.IsAdministrator();

        // Assert
        using (new AssertionScope())
        {
            principal.IsInRole(HopautRoles.Admin).Should().BeTrue("the role claim is present");
            isAdministrator.Should().BeFalse("an unauthenticated principal never acts as an administrator");
        }
    }

    private static ClaimsPrincipal Principal(bool authenticated, params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: authenticated ? "Bearer" : null));
}
