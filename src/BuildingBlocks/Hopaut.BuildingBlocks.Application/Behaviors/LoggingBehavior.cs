using Hopaut.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Hopaut.BuildingBlocks.Application.Behaviors;

/// <summary>Outermost pipeline behavior: logs every command and query and the error code of an expected failure.</summary>
public sealed class LoggingBehavior<TMessage, TResponse>(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : IResultBase
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;
        PipelineLog.Handling(logger, messageName);

        var response = await next(message, cancellationToken);

        if (response.IsSuccess)
        {
            PipelineLog.Handled(logger, messageName);
        }
        else
        {
            PipelineLog.HandledWithError(logger, messageName, response.Error.Code, response.Error.Type);
        }

        return response;
    }
}

internal static partial class PipelineLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Handling {MessageName}")]
    public static partial void Handling(ILogger logger, string messageName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Handled {MessageName}")]
    public static partial void Handled(ILogger logger, string messageName);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Handled {MessageName} with error {ErrorCode} ({ErrorType})")]
    public static partial void HandledWithError(ILogger logger, string messageName, string errorCode, ErrorType errorType);
}
