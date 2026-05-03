using System.Text.Json;
using FileConversionTool.Models;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Services;

/// <summary>
/// Orchestrates the full conversion pipeline:
///   1. Validate the request (file exists, formats match, output directory writable)
///   2. Resolve the correct converter via <see cref="ConverterFactory"/>
///   3. Invoke the converter
///   4. Return a structured <see cref="ConversionResult"/>
///
/// This service is the single seam between the UI (console) and the converters.
/// All error handling lives here so converters stay focused on conversion logic.
/// </summary>
public sealed class ConversionService
{
    private readonly ConverterFactory _factory;
    private readonly ILogger<ConversionService> _logger;

    public ConversionService(ConverterFactory factory, ILogger<ConversionService> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    public ConversionResult Convert(ConversionRequest request)
    {
        _logger.LogInformation(
            "ConversionService: processing {InputFormat} → {OutputFormat}",
            request.InputFormat, request.OutputFormat);

        // --- Validation ---
        if (!File.Exists(request.InputPath))
            return ConversionResult.Fail($"Input file not found: '{request.InputPath}'");

        var inputExtension = Path.GetExtension(request.InputPath).TrimStart('.').ToLowerInvariant();
        if (!inputExtension.Equals(request.InputFormat, StringComparison.OrdinalIgnoreCase))
        {
            return ConversionResult.Fail(
                $"File extension '.{inputExtension}' does not match expected format '{request.InputFormat}'.");
        }

        var outputDir = Path.GetDirectoryName(request.OutputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            try
            {
                Directory.CreateDirectory(outputDir);
                _logger.LogInformation("Created output directory: {Dir}", outputDir);
            }
            catch (Exception ex)
            {
                return ConversionResult.Fail($"Cannot create output directory '{outputDir}': {ex.Message}");
            }
        }

        // --- Converter resolution (no if/else — factory handles dispatch) ---
        var converter = _factory.GetConverter(request.InputFormat, request.OutputFormat);
        if (converter is null)
        {
            return ConversionResult.Fail(
                $"No converter available for {request.InputFormat.ToUpper()} → {request.OutputFormat.ToUpper()}. " +
                "Check supported conversions in the menu.");
        }

        // --- Execution ---
        try
        {
            converter.Convert(request.InputPath, request.OutputPath);
            return ConversionResult.Ok(request.OutputPath);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found during conversion.");
            return ConversionResult.Fail($"File not found: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Malformed JSON.");
            return ConversionResult.Fail($"Invalid JSON: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Conversion logic error.");
            return ConversionResult.Fail($"Conversion failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during conversion.");
            return ConversionResult.Fail($"Unexpected error: {ex.Message}");
        }
    }
}
