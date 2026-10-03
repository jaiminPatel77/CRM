using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Extensions;

/// <summary>
/// Extension methods to check whether specific level of log is enabled or not.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Indicate whether Trace level is enabled or not.
    /// </summary>
    /// <param name="logger">Represents a type used to perform logging</param>
    /// <returns>true if trace level logging is enabled.</returns>
    public static bool IsTraceEnabled(this ILogger logger)
    {
        return logger.IsEnabled(LogLevel.Trace);
    }

    /// <summary>
    /// Indicate whether Debug level is enabled or not.
    /// </summary>
    /// <param name="logger">Represents a type used to perform logging</param>
    /// <returns>true if Debug level logging is enabled.</returns>
    public static bool IsDebugEnabled(this ILogger logger)
    {
        return logger.IsEnabled(LogLevel.Debug);
    }

    /// <summary>
    /// Indicate whether Information level is enabled or not.
    /// </summary>
    /// <param name="logger">Represents a type used to perform logging</param>
    /// <returns>true if Information level logging is enabled.</returns>
    public static bool IsInformationEnabled(this ILogger logger)
    {
        return logger.IsEnabled(LogLevel.Information);
    }

    /// <summary>
    /// Indicate whether Warning level is enabled or not.
    /// </summary>
    /// <param name="logger">Represents a type used to perform logging</param>
    /// <returns>true if Warning level logging is enabled.</returns>
    public static bool IsWarningEnabled(this ILogger logger)
    {
        return logger.IsEnabled(LogLevel.Warning);
    }

    /// <summary>
    /// Indicate whether Error level is enabled or not.
    /// </summary>
    /// <param name="logger">Represents a type used to perform logging</param>
    /// <returns>true if Error level logging is enabled.</returns>
    public static bool IsErrorEnabled(this ILogger logger)
    {
        return logger.IsEnabled(LogLevel.Error);
    }

    /// <summary>
    /// Indicate whether Critical level is enabled or not.
    /// </summary>
    /// <param name="logger">Represents a type used to perform logging</param>
    /// <returns>true if Critical level logging is enabled.</returns>
    public static bool IsCriticalEnabled(this ILogger logger)
    {
        return logger.IsEnabled(LogLevel.Critical);
    }
}
