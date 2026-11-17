using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application;

/// <summary>Marks every command of the Attendance module; selects <see cref="AttendanceUnitOfWorkBehavior{TMessage,TResponse}"/>.</summary>
public interface IAttendanceCommand : IBaseCommand;

/// <summary>The Attendance module's unit of work, implemented by its DbContext.</summary>
public interface IAttendanceUnitOfWork : IUnitOfWork;

/// <summary>Saves the Attendance module's changes once, after a successful Attendance command.</summary>
public sealed class AttendanceUnitOfWorkBehavior<TMessage, TResponse>(IAttendanceUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
    where TMessage : IAttendanceCommand
    where TResponse : IResultBase, IFailureFactory<TResponse>;
