# Endpoint patterns (Hopaut)

Reference shapes for [SKILL.md](../SKILL.md). Verify package APIs (Asp.Versioning, Scalar, Testcontainers) against the versions pinned in `Directory.Packages.props` before copying.

## Contents
1. Host wiring
2. Error → problem details
3. Routes, DTOs and an endpoint
4. Current user
5. Cursor paging
6. Test factory
7. Endpoint test
8. OpenAPI snapshot test

## 1. Host wiring

```csharp
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddOpenApi("v1");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // /openapi/v1.json
    app.MapScalarApiReference();
}

var v1 = app.NewVersionedApi("Hopaut")
    .MapGroup("/api/v{version:apiVersion}")
    .HasApiVersion(1.0)
    .RequireAuthorization();

foreach (var module in app.Services.GetServices<IModuleEndpoints>())
    module.MapEndpoints(v1);
```

Prove with a test that `traceId` and `instance` appear on responses produced by `TypedResults.Problem`; if they do not, add them in an endpoint filter.

## 2. Error → problem details

```csharp
public static class ErrorHttpMapping
{
    extension(Error error)
    {
        public ProblemHttpResult ToProblem()
        {
            var status = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Rule => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError,
            };

            ProblemDetails problem = error is ValidationError validation
                ? new HttpValidationProblemDetails(validation.Errors.ToDictionary(e => ToCamelCasePath(e.Key), e => e.Value))
                : new ProblemDetails();

            problem.Status = status;
            problem.Title = ReasonPhrases.GetReasonPhrase(status);
            problem.Detail = status == StatusCodes.Status500InternalServerError ? null : error.Description;
            problem.Extensions["errorCode"] = error.Code;

            return TypedResults.Problem(problem);
        }
    }

    private static string ToCamelCasePath(string path) =>
        string.Join('.', path.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
```

## 3. Routes, DTOs and an endpoint

```csharp
public static class AttendanceRoutes
{
    public const string Attendees = "/posts/{postId:int}/attendees";
    public const string Attendee = "/posts/{postId:int}/attendees/{userId}";
    public const string MyAttendance = "/posts/{postId:int}/attendees/me";
}

// Modules.Attendance.Api/Contracts/V1
public enum AttendeeDecision { Accepted, Rejected }

public sealed record UpdateAttendeeStatusRequest(AttendeeDecision Status);

public sealed record AttendeeResponse(string UserId, string Status, DateTimeOffset RequestedAt, DateTimeOffset? DecidedAt);

public sealed class AttendanceModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var attendance = endpoints.MapGroup(string.Empty).WithTags("Attendance");

        attendance.MapPatch(AttendanceRoutes.Attendee, UpdateAttendeeStatus)
            .WithName("Attendance_UpdateAttendeeStatus")
            .WithSummary("Accept or reject a pending attendee. Post host only.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    /// <summary>Accepts or rejects a pending attendee of a post.</summary>
    /// <param name="postId">The post the attendee asked to join.</param>
    /// <param name="userId">The attendee.</param>
    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateAttendeeStatus(
        PostId postId,
        UserId userId,
        UpdateAttendeeStatusRequest request,
        ICurrentUser currentUser,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var decision = request.Status == AttendeeDecision.Accepted ? AttendanceDecision.Accept : AttendanceDecision.Reject;
        var result = await sender.Send(new DecideAttendanceCommand(postId, userId, decision, currentUser.Id), cancellationToken);

        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }
}
```

`PostId` and `UserId` bind straight from the route because they implement `IParsable<T>`.

## 4. Current user

```csharp
// BuildingBlocks.Application
public interface ICurrentUser
{
    UserId Id { get; }
}

// BuildingBlocks.Api; the claim type is part of the auth contract with BingoMobile.
internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public UserId Id =>
        accessor.HttpContext?.User.FindFirstValue(HopautClaimTypes.UserId) is { Length: > 0 } value
            ? new UserId(value)
            : throw new InvalidOperationException("ICurrentUser was used outside an authenticated request.");
}
```

## 5. Cursor paging

```csharp
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);

// Opaque keyset cursor: base64url of the last item's sort key and ID; never an offset.
public sealed record PostFeedCursor(DateTimeOffset StartsAt, int PostId)
{
    public string Encode() =>
        Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(this, JsonSerializerOptions.Web));

    public static bool TryDecode(string? cursor, [NotNullWhen(true)] out PostFeedCursor? decoded)
    {
        decoded = null;
        if (string.IsNullOrEmpty(cursor))
            return false;

        try
        {
            decoded = JsonSerializer.Deserialize<PostFeedCursor>(Base64Url.DecodeFromChars(cursor), JsonSerializerOptions.Web);
            return decoded is not null;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return false;
        }
    }
}
```

## 6. Test factory

```csharp
public sealed class HopautApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Use a postgis/postgis tag that matches the production Postgres major version.
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:<pg-major>-<postgis-version>")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use the Host's real connection-string keys (one per module if configured that way).
        builder.UseSetting("ConnectionStrings:Default", _database.GetConnectionString());
        builder.ConfigureTestServices(services => services
            .AddAuthentication(TestAuthHandler.Scheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { }));
    }

    public HttpClient CreateClientFor(string userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId);
        return client;
    }

    Task IAsyncLifetime.InitializeAsync() => _database.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => _database.DisposeAsync().AsTask();
}
```

## 7. Endpoint test

```csharp
public sealed class AttendanceEndpointsTests(HopautApiFactory factory) : IClassFixture<HopautApiFactory>
{
    [Fact]
    public async Task UpdateAttendeeStatus_WhenCallerIsNotHost_Returns403Problem()
    {
        var (postId, attendeeId) = await AttendanceSeed.PendingAttendeeAsync(factory); // arrange through the API or a seeding helper
        var client = factory.CreateClientFor("intruder");

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/posts/{postId}/attendees/{attendeeId}", new { status = "accepted" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("attendance.participation.not_post_host", (string?)problem?["errorCode"]);
    }
}
```

## 8. OpenAPI snapshot test

```csharp
public sealed class OpenApiSnapshotTests(HopautApiFactory factory) : IClassFixture<HopautApiFactory>
{
    private const string UpdateVariable = "HOPAUT_UPDATE_SNAPSHOTS";

    [Fact]
    public async Task OpenApiV1_MatchesCommittedSnapshot()
    {
        var generated = JsonNode.Parse(await factory.CreateClient().GetStringAsync("/openapi/v1.json"))
            ?? throw new InvalidOperationException("Empty OpenAPI document.");
        var snapshotDirectory = Path.Combine(RepositoryRoot(), "docs", "openapi");
        var snapshotFile = Path.Combine(snapshotDirectory, "v1.json");

        if (Environment.GetEnvironmentVariable(UpdateVariable) == "1")
        {
            Directory.CreateDirectory(snapshotDirectory);
            await File.WriteAllTextAsync(snapshotFile, generated.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            return;
        }

        Assert.True(File.Exists(snapshotFile), $"Missing {snapshotFile}. Run once with {UpdateVariable}=1 and commit it.");
        var committed = JsonNode.Parse(await File.ReadAllTextAsync(snapshotFile));
        Assert.True(JsonNode.DeepEquals(committed, generated),
            "The API contract changed: run the contract sync procedure, then refresh docs/openapi/v1.json in the same commit.");
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Bingo.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Bingo.sln not found above the test output folder.");
    }
}
```
