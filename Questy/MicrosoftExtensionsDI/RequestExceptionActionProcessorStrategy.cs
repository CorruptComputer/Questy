namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///   The strategy for processing request exceptions.
/// </summary>
public enum RequestExceptionActionProcessorStrategy
{
    /// <summary>
    ///   Apply the action processor only for unhandled exceptions.
    /// </summary>
    ApplyForUnhandledExceptions,

    /// <summary>
    ///   Apply the action processor for all exceptions.
    /// </summary>
    ApplyForAllExceptions
}