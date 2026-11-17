using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.BugReports.Application;

/// <summary>Marks every command of the BugReports module; selects <see cref="BugReportsUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IBugReportsCommand : IBaseCommand;

/// <summary>The BugReports module's unit of work, implemented by its DbContext.</summary>
public interface IBugReportsUnitOfWork : IUnitOfWork;

/// <summary>Saves the BugReports module's changes once, after a successful BugReports command.</summary>
public sealed class BugReportsUnitOfWorkBehavior<TMessage, TResponse>(IBugReportsUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IBugReportsCommand
    where TResponse : IResultBase, IFailureFactory<TResponse>;
