using System.Text.RegularExpressions;

namespace ErrorService.Application.Helpers;

/// <summary>
/// Sanitizes stack traces before storage to prevent information disclosure.
/// Removes absolute file system paths that can reveal server directory structure.
/// </summary>
public static partial class StackTraceSanitizer
{
    /// <summary>
    /// Sanitizes a stack trace by removing absolute file paths while preserving
    /// the file name, line number, and method information.
    /// </summary>
    /// <param name="stackTrace">The raw stack trace string.</param>
    /// <returns>Sanitized stack trace with absolute paths removed.</returns>
    public static string? Sanitize(string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
            return stackTrace;

        var result = stackTrace;

        // Remove Unix absolute paths: /Users/xxx/..., /home/xxx/..., /root/..., /var/..., /opt/...
        // Keeps just the filename: /Users/dev/project/src/Services/MyService.cs:line 42 → MyService.cs:line 42
        result = UnixPathRegex().Replace(result, "$1");

        // Remove Windows absolute paths: C:\Users\xxx\..., D:\Projects\...
        // Keeps just the filename: C:\Users\dev\project\src\Services\MyService.cs:line 42 → MyService.cs:line 42
        result = WindowsPathRegex().Replace(result, "$1");

        // Remove "in " prefix paths that .NET includes before filenames
        // e.g., "in /app/src/Services/MyService.cs:line 42" → "in MyService.cs:line 42"
        result = InPathRegex().Replace(result, "in $1");

        return result;
    }

    // Matches Unix paths like /Users/x/project/src/File.cs or /home/user/app/File.cs
    // Captures the filename at the end
    [GeneratedRegex(@"(?:/(?:Users|home|root|var|opt|app|src|tmp)[^\s]*/)([^\s/]+\.cs)")]
    private static partial Regex UnixPathRegex();

    // Matches Windows paths like C:\Users\x\project\src\File.cs
    // Captures the filename at the end
    [GeneratedRegex(@"(?:[A-Za-z]:\\[^\s]*\\)([^\s\\]+\.cs)")]
    private static partial Regex WindowsPathRegex();

    // Matches remaining "in /some/path/File.cs" patterns
    [GeneratedRegex(@"in\s+(?:[/\\][^\s]*[/\\])([^\s/\\]+\.cs)")]
    private static partial Regex InPathRegex();
}
