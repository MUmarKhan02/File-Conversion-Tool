using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Services;

/// <summary>
/// Resolves the correct <see cref="IFileConverter"/> for a given input/output format pair.
///
/// Design decision — Dictionary-based dispatch over if/else chains:
///   ✓ O(1) lookup regardless of how many converters are registered
///   ✓ Adding a new converter = one new DI registration, zero factory changes
///   ✓ The factory is closed for modification (OCP) but open for extension via DI
///
/// All registered IFileConverter implementations are injected by the DI container.
/// The factory indexes them by a (inputFormat, outputFormat) composite key at startup.
/// </summary>
public sealed class ConverterFactory
{
    // Key: "inputFormat→outputFormat" (e.g. "json→csv")
    private readonly IReadOnlyDictionary<string, IFileConverter> _converters;
    private readonly ILogger<ConverterFactory> _logger;

    public ConverterFactory(
        IEnumerable<IFileConverter> converters,
        ILogger<ConverterFactory> logger)
    {
        _logger = logger;

        // Build the lookup table once at construction time
        _converters = converters.ToDictionary(
            c => BuildKey(c.InputFormat, c.OutputFormat),
            c => c,
            StringComparer.OrdinalIgnoreCase
        );

        _logger.LogInformation(
            "ConverterFactory initialized with {Count} converter(s): {Keys}",
            _converters.Count,
            string.Join(", ", _converters.Keys));
    }

    /// <summary>
    /// Returns the converter for the specified format pair, or null if none is registered.
    /// Callers should check for null and present a user-friendly error.
    /// </summary>
    public IFileConverter? GetConverter(string inputFormat, string outputFormat)
    {
        var key = BuildKey(inputFormat, outputFormat);

        if (_converters.TryGetValue(key, out var converter))
        {
            _logger.LogDebug("Resolved converter for {Key}: {Type}", key, converter.GetType().Name);
            return converter;
        }

        _logger.LogWarning("No converter registered for {Key}.", key);
        return null;
    }

    /// <summary>Returns all supported (input → output) format pairs.</summary>
    public IEnumerable<(string Input, string Output)> GetSupportedConversions() =>
        _converters.Values.Select(c => (c.InputFormat, c.OutputFormat));

    private static string BuildKey(string input, string output) =>
        $"{input.ToLowerInvariant()}→{output.ToLowerInvariant()}";
}
