using System;

namespace Crm.Infrastructure.Extensions;

/// <summary>
/// Extension methods related to any exception.
/// </summary>
public static class ExceptionExtension
{
    /// <summary>
    /// Return error string by con-cat exception error message and it's inner exception detail if any using
    /// given separator 
    /// </summary>
    /// <param name="exception">Actual exception</param>
    /// <param name="separator">Separator to be used to con-cat the inner exception if any.</param>
    /// <returns>Exception error detail as string.</returns>
    public static string ToErrorMessage(this Exception exception, string? separator = null)
    {
        var errorInfo = exception.Message;
        var innerException = exception.InnerException;
        var lastMessage = exception.Message;
        if (separator == null)
        {
            separator = "! ";
        }
        while (innerException != null)
        {
            if (lastMessage != innerException.Message)
            {
                errorInfo = string.Join(separator, errorInfo, innerException.Message);
            }
            lastMessage = innerException.Message;
            innerException = innerException.InnerException;
        }
        return errorInfo;
    }
}
