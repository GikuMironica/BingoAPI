using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Innermost pipeline behavior of a module's commands: saves the module's unit of work exactly once, after the handler
/// returned a successful result. A failed result saves nothing, so a rejected command leaves no trace. A save that
/// collides with a concurrent change (<see cref="ConcurrencyConflictException"/>) becomes the failed result
/// <see cref="UnitOfWorkErrors.ConcurrencyConflict"/> (409).
/// Each module derives a sealed behavior constrained to its command marker and resolving its own unit of work.
/// </summary>
public abstract class UnitOfWorkBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
    where TResponse : IResultBase, IFailureFactory<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);
        if (response.IsFailure)
        {
            return response;
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return TResponse.Failure(UnitOfWorkErrors.ConcurrencyConflict);
        }

        return response;
    }
}
