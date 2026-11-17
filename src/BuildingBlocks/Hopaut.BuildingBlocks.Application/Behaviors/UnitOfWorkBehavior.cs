using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Innermost pipeline behavior of a module's commands: saves the module's unit of work exactly once, after the handler
/// returned a successful result. A failed result saves nothing, so a rejected command leaves no trace.
/// Each module derives a sealed behavior constrained to its command marker and resolving its own unit of work.
/// </summary>
public abstract class UnitOfWorkBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
    where TResponse : IResultBase
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);
        if (response.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
