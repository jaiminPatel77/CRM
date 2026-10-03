using System.Text.RegularExpressions;

namespace Crm.Api.Middleware;

public class InputSanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputSanitizationMiddleware> _logger;

    private static readonly Regex ScriptTagPattern = new(
        @"<script[^>]*>.*?</script>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex EventHandlerPattern = new(
        @"\bon\w+\s*=",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex IframePattern = new(
        @"<iframe[^>]*>.*?</iframe>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex HtmlTagPattern = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    public InputSanitizationMiddleware(RequestDelegate next, ILogger<InputSanitizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.QueryString.HasValue)
        {
            var queryString = context.Request.QueryString.Value;
            if (!string.IsNullOrEmpty(queryString))
            {
                var sanitized = SanitizeQueryString(queryString);
                if (sanitized != queryString)
                {
                    _logger.LogWarning("Query string sanitized for potential XSS at {Path}", context.Request.Path);
                    context.Request.QueryString = new QueryString(sanitized);
                }
            }
        }

        await _next(context);
    }

    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sanitized = input;
        sanitized = ScriptTagPattern.Replace(sanitized, string.Empty);
        sanitized = IframePattern.Replace(sanitized, string.Empty);
        sanitized = EventHandlerPattern.Replace(sanitized, string.Empty);

        return sanitized;
    }

    public static string SanitizeQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString))
            return queryString;

        return HtmlTagPattern.Replace(SanitizeInput(queryString), string.Empty);
    }
}

public static class InputSanitizationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputSanitization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<InputSanitizationMiddleware>();
    }
}