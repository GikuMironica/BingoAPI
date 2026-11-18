using System.Security.Claims;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.BuildingBlocks.UnitTests.Infrastructure;

public sealed class ResourceOwnerPolicyTests
{
    private const string OwnerOrAdministrator = "tests.document.owner";
    private const string OwnerOnly = "tests.document.owner-only";

    private static readonly UserId Owner = UserId.From("owner-1");

    [Theory]
    [InlineData(OwnerOrAdministrator)]
    [InlineData(OwnerOnly)]
    public async Task GivenTheOwner_WhenAuthorized_ThenThePolicySucceeds(string policy)
    {
        // Arrange
        var authorization = AuthorizationService();

        // Act
        var result = await authorization.AuthorizeAsync(Caller(Owner.Value), new Document(Owner), policy);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(OwnerOrAdministrator)]
    [InlineData(OwnerOnly)]
    public async Task GivenAnotherUser_WhenAuthorized_ThenThePolicyFails(string policy)
    {
        // Arrange
        var authorization = AuthorizationService();

        // Act
        var result = await authorization.AuthorizeAsync(Caller("someone-else"), new Document(Owner), policy);

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(OwnerOrAdministrator, HopautRoles.Admin, true)]
    [InlineData(OwnerOrAdministrator, HopautRoles.SuperAdmin, true)]
    [InlineData(OwnerOnly, HopautRoles.Admin, false)]
    [InlineData(OwnerOrAdministrator, HopautRoles.User, false)]
    public async Task GivenACallerInARole_WhenAuthorizedOnSomeoneElsesResource_ThenOnlyAllowedAdministratorsSucceed(
        string policy, string role, bool expected)
    {
        // Arrange
        var authorization = AuthorizationService();

        // Act
        var result = await authorization.AuthorizeAsync(Caller("moderator-1", role), new Document(Owner), policy);

        // Assert
        result.Succeeded.Should().Be(expected);
    }

    [Fact]
    public async Task GivenAnUnauthenticatedPrincipalCarryingTheOwnersId_WhenAuthorized_ThenThePolicyFails()
    {
        // Arrange
        var authorization = AuthorizationService();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity([new Claim(HopautClaimTypes.UserId, Owner.Value)]));

        // Act
        var result = await authorization.AuthorizeAsync(anonymous, new Document(Owner), OwnerOrAdministrator);

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task GivenAResourceWithoutOwner_WhenAuthorized_ThenOnlyAnAdministratorSucceeds()
    {
        // Arrange
        var authorization = AuthorizationService();
        var orphan = new Document(null);

        // Act
        var asUser = await authorization.AuthorizeAsync(Caller("user-1"), orphan, OwnerOrAdministrator);
        var asAdministrator = await authorization.AuthorizeAsync(Caller("admin-1", HopautRoles.Admin), orphan, OwnerOrAdministrator);

        // Assert
        using (new AssertionScope())
        {
            asUser.Succeeded.Should().BeFalse();
            asAdministrator.Succeeded.Should().BeTrue();
        }
    }

    [Fact]
    public void GivenTheSamePolicyNameRegisteredTwice_WhenTheAuthorizationServiceIsBuilt_ThenItThrows()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddResourceOwnerPolicy<Document>(OwnerOnly, document => document.Owner, administratorsAllowed: false)
            .AddResourceOwnerPolicy<Document>(OwnerOnly, document => document.Owner, administratorsAllowed: true)
            .BuildServiceProvider();

        // Act
        var act = () => services.GetRequiredService<IAuthorizationService>();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage($"*'{OwnerOnly}'*more than once*");
    }

    private static IAuthorizationService AuthorizationService() =>
        new ServiceCollection()
            .AddLogging()
            .AddResourceOwnerPolicy<Document>(OwnerOrAdministrator, document => document.Owner, administratorsAllowed: true)
            .AddResourceOwnerPolicy<Document>(OwnerOnly, document => document.Owner, administratorsAllowed: false)
            .BuildServiceProvider()
            .GetRequiredService<IAuthorizationService>();

    private static ClaimsPrincipal Caller(string userId, params string[] roles) =>
        new(new ClaimsIdentity(
            [new Claim(HopautClaimTypes.UserId, userId), .. roles.Select(role => new Claim(ClaimTypes.Role, role))],
            authenticationType: "Bearer"));

    private sealed record Document(UserId? Owner);
}
