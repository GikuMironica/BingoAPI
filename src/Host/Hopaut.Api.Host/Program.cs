using System.Threading.RateLimiting;
using Hangfire;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.BuildingBlocks.Api.Security;
using Hopaut.BuildingBlocks.Application;
using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.BuildingBlocks.Application.Caching;
using Hopaut.BuildingBlocks.Infrastructure.Caching;
using Hopaut.BuildingBlocks.Infrastructure.Hangfire;
using Hopaut.BuildingBlocks.Infrastructure.Outbox;
using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Announcements.Api;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Attendance.Api;
using Hopaut.Modules.Attendance.Application;
using Hopaut.Modules.BugReports.Api;
using Hopaut.Modules.BugReports.Application;
using Hopaut.Modules.Identity.Api;
using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Media.Api;
using Hopaut.Modules.Moderation.Api;
using Hopaut.Modules.Moderation.Application;
using Hopaut.Modules.Notifications.Api;
using Hopaut.Modules.Payments.Api;
using Hopaut.Modules.Posts.Api;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Ratings.Api;
using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Users.Api;
using Hopaut.Modules.Users.Application;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ── OpenTelemetry ──
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Hopaut.Api"))
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

// ── Rate Limiting ──
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 7;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.QueueLimit = 0;
    });
});

// ── Health Checks ──
builder.Services.AddHealthChecks();

// ── Time (handlers take now from TimeProvider; tests substitute FakeTimeProvider) ──
builder.Services.AddSingleton(TimeProvider.System);

// ── ProblemDetails (errorCode + traceId on every problem; unhandled exceptions → 500 without internals) ──
builder.Services.AddHopautProblemDetails();

// ── Current user (ICurrentUser for handlers) ──
builder.Services.AddHopautCurrentUser();

// ── Authorization (required by UseAuthorization and RequireAuthorization; JWT bearer authentication is plan Step 7.7) ──
builder.Services.AddAuthorization();

// ── Caching ──
builder.Services.AddHopautCaching(builder.Configuration);

// ── Integration Event Publishing (transitional shared outbox, replaced in plan Step 9.1) ──
builder.Services.AddHopautOutbox(builder.Configuration);

// ── Hangfire ──
builder.Services.AddHopautHangfire(builder.Configuration);

// ── Role Seeder ──
builder.Services.AddHostedService<Hopaut.Api.Host.RoleSeederHostedService>();

// ── Mediator (source-generated; the generator reads these literal lists at compile time) ──
// Scoped: handlers and behaviors depend on the scoped module DbContexts. List every assembly with messages or handlers
// (Domain: domain events; Application: commands, queries, handlers; Infrastructure: domain-event handlers).
// Pipeline, outer to inner: Logging → Validation → Caching (ICachedQuery) → the module's unit of work (its command marker).
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies =
    [
        typeof(IIdentityCommand),
        typeof(IUsersCommand),
        typeof(Hopaut.Modules.Media.Application.IMediaCommand),
        typeof(Hopaut.Modules.Posts.Domain.Post),
        typeof(IPostsCommand),
        typeof(Hopaut.Modules.Posts.Infrastructure.PostsModuleDbContext),
        typeof(Hopaut.Modules.Attendance.Domain.Participation),
        typeof(IAttendanceCommand),
        typeof(Hopaut.Modules.Attendance.Infrastructure.AttendanceModuleDbContext),
        typeof(Hopaut.Modules.Announcements.Domain.Announcement),
        typeof(IAnnouncementsCommand),
        typeof(Hopaut.Modules.Announcements.Infrastructure.AnnouncementsModuleDbContext),
        typeof(Hopaut.Modules.Ratings.Domain.Rating),
        typeof(IRatingsCommand),
        typeof(Hopaut.Modules.Ratings.Infrastructure.RatingsModuleDbContext),
        typeof(IModerationCommand),
        typeof(IBugReportsCommand),
    ];
    options.PipelineBehaviors =
    [
        typeof(LoggingBehavior<,>),
        typeof(ValidationBehavior<,>),
        typeof(CachingBehavior<,>),
        typeof(IdentityUnitOfWorkBehavior<,>),
        typeof(UsersUnitOfWorkBehavior<,>),
        typeof(PostsUnitOfWorkBehavior<,>),
        typeof(AttendanceUnitOfWorkBehavior<,>),
        typeof(AnnouncementsUnitOfWorkBehavior<,>),
        typeof(RatingsUnitOfWorkBehavior<,>),
        typeof(ModerationUnitOfWorkBehavior<,>),
        typeof(BugReportsUnitOfWorkBehavior<,>),
    ];
});

// ── Modules ──
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddMediaModule(builder.Configuration);
builder.Services.AddPostsModule(builder.Configuration);
builder.Services.AddAttendanceModule(builder.Configuration);
builder.Services.AddAnnouncementsModule(builder.Configuration);
builder.Services.AddRatingsModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddModerationModule(builder.Configuration);
builder.Services.AddBugReportsModule(builder.Configuration);
builder.Services.AddPaymentsModule(builder.Configuration);

var moduleDbContexts = DatabaseMigrator.RegisteredDbContexts(builder.Services);

var app = builder.Build();

// ── Database migrations: opt-in for Development and tests; production applies reviewed scripts ──
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await DatabaseMigrator.MigrateAsync(app.Services, moduleDbContexts, app.Logger, CancellationToken.None);
}

// ── Middleware pipeline ──
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapHangfireDashboard("/admin/hangfire");

// ── Module endpoints ──
app.MapIdentityEndpoints();
app.MapUsersEndpoints();
app.MapMediaEndpoints();
app.MapPostsEndpoints();
app.MapAttendanceEndpoints();
app.MapAnnouncementsEndpoints();
app.MapRatingsEndpoints();
app.MapModerationEndpoints();
app.MapBugReportsEndpoints();
app.MapPaymentsEndpoints();

app.Run();

// Exposes the implicit top-level Program class to WebApplicationFactory<Program> in tests/Hopaut.IntegrationTests.
public partial class Program;
