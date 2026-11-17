using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application;

/// <summary>Marks every command of the Announcements module; selects <see cref="AnnouncementsUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IAnnouncementsCommand : IBaseCommand;

/// <summary>The Announcements module's unit of work, implemented by its DbContext.</summary>
public interface IAnnouncementsUnitOfWork : IUnitOfWork;

/// <summary>Saves the Announcements module's changes once, after a successful Announcements command.</summary>
public sealed class AnnouncementsUnitOfWorkBehavior<TMessage, TResponse>(IAnnouncementsUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IAnnouncementsCommand
    where TResponse : IResultBase;
