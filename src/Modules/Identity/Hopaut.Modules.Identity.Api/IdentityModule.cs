using Hopaut.BuildingBlocks.Api;
using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Identity.Application.Commands.ChangePassword;
using Hopaut.Modules.Identity.Application.Commands.ConfirmEmail;
using Hopaut.Modules.Identity.Application.Commands.ForgotPassword;
using Hopaut.Modules.Identity.Application.Commands.Login;
using Hopaut.Modules.Identity.Application.Commands.RefreshToken;
using Hopaut.Modules.Identity.Application.Commands.Register;
using Hopaut.Modules.Identity.Application.Commands.ResetPassword;
using MediatR;
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

        group.MapPost("/register", async (RegisterUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/login", async (LoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/refresh", async (RefreshTokenCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapGet("/confirm-email", async (string userId, string token, ISender sender) =>
        {
            var result = await sender.Send(new ConfirmEmailCommand(userId, token));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/forgot-password", async (ForgotPasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result); // Always 200 to not reveal user existence
        });

        group.MapPost("/reset-password", async (ResetPasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/change-password", async (ChangePasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization();
    }
}
