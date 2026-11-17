using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application;

/// <summary>Marks every command of the Ratings module; selects <see cref="RatingsUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IRatingsCommand : IBaseCommand;

/// <summary>The Ratings module's unit of work, implemented by its DbContext.</summary>
public interface IRatingsUnitOfWork : IUnitOfWork;

/// <summary>Saves the Ratings module's changes once, after a successful Ratings command.</summary>
public sealed class RatingsUnitOfWorkBehavior<TMessage, TResponse>(IRatingsUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IRatingsCommand
    where TResponse : IResultBase;
