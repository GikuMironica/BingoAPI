using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Hangfire;
using Hopaut.BuildingBlocks.Infrastructure.Hangfire;
using Hopaut.BuildingBlocks.Infrastructure.Caching;
using Hopaut.Modules.Identity.Api;
using Hopaut.Modules.Users.Api;
using Hopaut.Modules.Media.Api;
using Hopaut.Modules.Posts.Api;
using Hopaut.Modules.Attendance.Api;
using Hopaut.Modules.Announcements.Api;
using Hopaut.Modules.Ratings.Api;
using Hopaut.Modules.Notifications.Api;
using Hopaut.Modules.Moderation.Api;
using Hopaut.Modules.BugReports.Api;
using Hopaut.Modules.Payments.Api;

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

// ── ProblemDetails ──
builder.Services.AddProblemDetails();

// ── Caching ──
builder.Services.AddHopautCaching(builder.Configuration);

// ── Hangfire ──
builder.Services.AddHopautHangfire(builder.Configuration);

// ── Role Seeder ──
builder.Services.AddHostedService<Hopaut.Api.Host.RoleSeederHostedService>();

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

var app = builder.Build();

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
