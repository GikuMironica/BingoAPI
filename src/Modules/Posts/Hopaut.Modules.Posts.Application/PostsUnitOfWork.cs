using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application;

/// <summary>Marks every command of the Posts module; selects <see cref="PostsUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IPostsCommand : IBaseCommand;

/// <summary>The Posts module's unit of work, implemented by its DbContext.</summary>
public interface IPostsUnitOfWork : IUnitOfWork;

/// <summary>Saves the Posts module's changes once, after a successful Posts command.</summary>
public sealed class PostsUnitOfWorkBehavior<TMessage, TResponse>(IPostsUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IPostsCommand
    where TResponse : IResultBase;
