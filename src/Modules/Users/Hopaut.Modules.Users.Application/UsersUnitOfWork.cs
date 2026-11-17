using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Users.Application;

/// <summary>Marks every command of the Users module; selects <see cref="UsersUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IUsersCommand : IBaseCommand;

/// <summary>The Users module's unit of work, implemented by its DbContext.</summary>
public interface IUsersUnitOfWork : IUnitOfWork;

/// <summary>Saves the Users module's changes once, after a successful Users command.</summary>
public sealed class UsersUnitOfWorkBehavior<TMessage, TResponse>(IUsersUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IUsersCommand
    where TResponse : IResultBase;
