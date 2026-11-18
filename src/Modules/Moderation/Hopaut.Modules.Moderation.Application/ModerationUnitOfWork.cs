using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application;

/// <summary>Marks every command of the Moderation module; selects <see cref="ModerationUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IModerationCommand : IBaseCommand;

/// <summary>The Moderation module's unit of work, implemented by its DbContext.</summary>
public interface IModerationUnitOfWork : IUnitOfWork;

/// <summary>Saves the Moderation module's changes once, after a successful Moderation command.</summary>
public sealed class ModerationUnitOfWorkBehavior<TMessage, TResponse>(IModerationUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IModerationCommand
    where TResponse : IResultBase, IFailureFactory<TResponse>;
