using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Identity.Application.Commands.ChangePassword;
using Hopaut.Modules.Identity.Application.Commands.ConfirmEmail;
using Hopaut.Modules.Identity.Application.Commands.ForgotPassword;
using Hopaut.Modules.Identity.Application.Commands.Login;
using Hopaut.Modules.Identity.Application.Commands.RefreshToken;
using Hopaut.Modules.Identity.Application.Commands.Register;
using Hopaut.Modules.Identity.Application.Commands.ResetPassword;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hopaut.Modules.Identity.Api;

public sealed class IdentityModule : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/identity")
            .WithTags("Identity");

        group.MapPost("/register", async (RegisterUserCommand command, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(command, cancellationToken)).ToLegacyAuthResponse());

        group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(command, cancellationToken)).ToLegacyAuthResponse());

        group.MapPost("/refresh", async (RefreshTokenCommand command, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(command, cancellationToken)).ToLegacyAuthResponse());

        group.MapGet("/confirm-email", async (string userId, string token, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(new ConfirmEmailCommand(userId, token), cancellationToken)).ToLegacyAuthResponse());

        group.MapPost("/forgot-password", async (ForgotPasswordCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(result.Value) // Always 200 to not reveal user existence
                : result.ToLegacyAuthResponse();
        });

        group.MapPost("/reset-password", async (ResetPasswordCommand command, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(command, cancellationToken)).ToLegacyAuthResponse());

        group.MapPost("/change-password", async (ChangePasswordCommand command, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(command, cancellationToken)).ToLegacyAuthResponse())
            .RequireAuthorization();
    }
}

/// <summary>
/// Today's Identity wire format (D-007) until plan Step 10.2: 200 with the <see cref="AuthResult"/> when it succeeded,
/// otherwise 400 with it. Pipeline failures (validation) become a failed <see cref="AuthResult"/> listing their messages.
/// </summary>
internal static class LegacyAuthResponses
{
    public static IResult ToLegacyAuthResponse(this Result<AuthResult> result)
    {
        if (result.IsFailure)
        {
            return Results.BadRequest(AuthResult.Fail(MessagesOf(result.Error)));
        }

        return result.Value.Success ? Results.Ok(result.Value) : Results.BadRequest(result.Value);
    }

    private static string[] MessagesOf(Error error) =>
        error is ValidationError validation
            ? validation.Errors.SelectMany(field => field.Value).ToArray()
            : [error.Description];
}
