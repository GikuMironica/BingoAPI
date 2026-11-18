using Mediator;

namespace Hopaut.Modules.Media.Application;

/// <summary>
/// Marks every command of the Media module. Media has no persistence yet (finding F-048, plan Step 8.10), so no
/// unit-of-work behavior is attached to it.
/// </summary>
public interface IMediaCommand : IBaseCommand;
