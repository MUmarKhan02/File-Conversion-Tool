namespace FileConversionTool.Models;

/// <summary>
/// Value object representing a single conversion job.
/// Keeping request data in its own model decouples the UI layer
/// from the service/converter layers.
/// </summary>
public sealed record ConversionRequest(
    string InputPath,
    string OutputPath,
    string InputFormat,
    string OutputFormat
);
