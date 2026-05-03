namespace FileConversionTool.Models;

/// <summary>
/// Outcome of a conversion attempt.
/// Using a result object (instead of exceptions bubbling to the UI)
/// keeps the console layer simple — it just reads Success/Message.
/// </summary>
public sealed record ConversionResult(
    bool Success,
    string Message,
    string? OutputPath = null
)
{
    public static ConversionResult Ok(string outputPath) =>
        new(true, "Conversion completed successfully.", outputPath);

    public static ConversionResult Fail(string reason) =>
        new(false, reason);
}
