using System.Security.Claims;
using Hopaut.BuildingBlocks.Api.Security;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.BuildingBlocks.UnitTests.Api;

public sealed class AuthorizationServiceResourceAuthorizerTests
{
    private const string Policy = "tests.note.owner";

    private static readonly UserId Author = UserId.From("author-1");

    [Fact]
    public async Task GivenTheRequestPrincipalOwnsTheResource_WhenAuthorized_ThenItIsAuthorized()
    {
        // Arrange
        var authorizer = Authorizer(new DefaultHttpContext { User = Caller(Author.Value) });

        // Act
        var authorized = await authorizer.IsAuthorizedAsync(new Note(Author), Policy, TestContext.Current.CancellationToken);

        // Assert
        authorized.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAnotherUsersRequest_WhenAuthorized_ThenItIsDenied()
    {
        // Arrange
        var authorizer = Authorizer(new DefaultHttpContext { User = Caller("reader-1") });

        // Act
        var authorized = await authorizer.IsAuthorizedAsync(new Note(Author), Policy, TestContext.Current.CancellationToken);

        // Assert
        authorized.Should().BeFalse();
    }

    [Fact]
    public async Task GivenNoCurrentRequest_WhenAuthorized_ThenTheAnonymousCallerIsDenied()
    {
        // Arrange
        var authorizer = Authorizer(httpContext: null);

        // Act
        var authorized = await authorizer.IsAuthorizedAsync(new Note(Author), Policy, TestContext.Current.CancellationToken);

        // Assert
        authorized.Should().BeFalse();
    }

    [Fact]
    public async Task GivenAnUnregisteredPolicy_WhenAuthorized_ThenItThrows()
    {
        // Arrange
        var authorizer = Authorizer(new DefaultHttpContext { User = Caller(Author.Value) });

        // Act
        var act = () => authorizer.IsAuthorizedAsync(new Note(Author), "tests.unknown", TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GivenACancelledToken_WhenAuthorized_ThenItThrowsOperationCanceled()
    {
        // Arrange
        var authorizer = Authorizer(new DefaultHttpContext { User = Caller(Author.Value) });

        // Act
        var act = () => authorizer.IsAuthorizedAsync(new Note(Author), Policy, new CancellationToken(canceled: true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private static AuthorizationServiceResourceAuthorizer Authorizer(HttpContext? httpContext)
    {
        var authorization = new ServiceCollection()
            .AddLogging()
            .AddResourceOwnerPolicy<Note>(Policy, note => note.Author, administratorsAllowed: false)
            .BuildServiceProvider()
            .GetRequiredService<IAuthorizationService>();
        return new AuthorizationServiceResourceAuthorizer(authorization, new HttpContextAccessor { HttpContext = httpContext });
    }

    private static ClaimsPrincipal Caller(string userId) =>
        new(new ClaimsIdentity([new Claim(HopautClaimTypes.UserId, userId)], authenticationType: "Bearer"));

    private sealed record Note(UserId Author);
}
