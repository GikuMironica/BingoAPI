using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Identity.Application;

/// <summary>Marks every command of the Identity module; selects <see cref="IdentityUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IIdentityCommand : IBaseCommand;

/// <summary>
/// The Identity module's unit of work, implemented by its DbContext. ASP.NET Core Identity's stores still save their own
/// changes until the Identity port of plan Step 8.9 (finding F-053); refresh tokens are saved by this unit of work.
/// </summary>
public interface IIdentityUnitOfWork : IUnitOfWork;

/// <summary>Saves the Identity module's changes once, after a successful Identity command.</summary>
public sealed class IdentityUnitOfWorkBehavior<TMessage, TResponse>(IIdentityUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IIdentityCommand
    where TResponse : IResultBase, IFailureFactory<TResponse>;
