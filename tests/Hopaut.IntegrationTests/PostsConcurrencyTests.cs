using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Domain;
using Hopaut.Modules.Posts.Infrastructure;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 8.2: a post is one consistency boundary guarded by PostgreSQL <c>xmin</c>, children included.</summary>
[Collection(HostCollection.Name)]
public sealed class PostsConcurrencyTests(HopautApiFactory factory)
{
    [Fact]
    public async Task GivenTwoEditorsOfOnePost_WhenTheSecondSavesAChildOnlyChange_ThenItIsAConflictAndNothingOfItIsSaved()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var postId = await CreatePostAsync("concurrency-host", ct);
        await using var first = factory.Services.CreateAsyncScope();
        await using var second = factory.Services.CreateAsyncScope();
        var firstPost = await first.ServiceProvider.GetRequiredService<IPostRepository>().GetByIdAsync(postId, ct);
        var secondPost = await second.ServiceProvider.GetRequiredService<IPostRepository>().GetByIdAsync(postId, ct);
        firstPost!.Disable();
        await first.ServiceProvider.GetRequiredService<IPostsUnitOfWork>().SaveChangesAsync(ct);
        secondPost!.AttachPicture("https://cdn.hopaut.app/late.webp");

        // Act
        var act = () => second.ServiceProvider.GetRequiredService<IPostsUnitOfWork>().SaveChangesAsync(ct);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyConflictException>();
        await using var verify = factory.Services.CreateAsyncScope();
        var pictures = await verify.ServiceProvider.GetRequiredService<PostsModuleDbContext>().Pictures.CountAsync(p => p.PostId == postId, ct);
        pictures.Should().Be(0);
    }

    [Fact]
    public async Task GivenAChildOnlyChange_WhenSaved_ThenThePostsVersionMovesOn()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var postId = await CreatePostAsync("version-host", ct);
        await using var scope = factory.Services.CreateAsyncScope();
        var post = await scope.ServiceProvider.GetRequiredService<IPostRepository>().GetByIdAsync(postId, ct);
        var before = post!.Version;
        post.AttachPicture("https://cdn.hopaut.app/new.webp");

        // Act
        await scope.ServiceProvider.GetRequiredService<IPostsUnitOfWork>().SaveChangesAsync(ct);

        // Assert
        await using var verify = factory.Services.CreateAsyncScope();
        var stored = await verify.ServiceProvider.GetRequiredService<PostsModuleDbContext>().Posts.AsNoTracking().SingleAsync(p => p.Id == postId, ct);
        using (new AssertionScope())
        {
            before.Should().NotBe(0u);
            stored.Version.Should().NotBe(before);
        }
    }

    private async Task<PostId> CreatePostAsync(string host, CancellationToken ct)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Create(host + "-" + Guid.NewGuid().ToString("N")));
        using var response = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            eventTime = new DateTimeOffset(2030, 1, 9, 18, 0, 0, TimeSpan.Zero),
            endTime = new DateTimeOffset(2030, 1, 9, 23, 0, 0, TimeSpan.Zero),
            userId = new { value = host },
            location = new { entityName = "Venue", city = "Berlin", region = (string?)null, address = "Main St 1", country = "DE", longitude = 13.405, latitude = 52.52 },
            @event = new { eventType = (int)EventType.HouseParty, entrancePrice = (double?)null, currency = (int?)null, title = "Concurrency", description = "Bring snacks and good vibes", requirements = (string?)null, slots = 25, typeSpecificData = (string?)null },
        }, ct);
        response.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return PostId.From(body.RootElement.GetProperty("postId").GetInt32());
    }
}
