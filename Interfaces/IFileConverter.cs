namespace FileConversionTool.Interfaces;

/// <summary>
/// Core abstraction for all file converters.
///
/// Design decision: Using an interface (not a base class) keeps converters
/// completely decoupled. Each converter is a self-contained unit that only
/// needs to know its input/output format and how to perform the conversion.
///
/// To add a new converter:
///   1. Create a new class implementing IFileConverter
///   2. Register it in Program.cs DI setup
///   → No other code needs to change (Open/Closed Principle)
/// </summary>
public interface IFileConverter
{
    /// <summary>File extension this converter accepts (e.g. "json", "csv").</summary>
    string InputFormat { get; }

    /// <summary>File extension this converter produces (e.g. "csv", "pdf").</summary>
    string OutputFormat { get; }

    /// <summary>
    /// Performs the conversion from <paramref name="inputPath"/> to <paramref name="outputPath"/>.
    /// Implementations should throw <see cref="FileNotFoundException"/> or
    /// <see cref="InvalidOperationException"/> for predictable error handling upstream.
    /// </summary>
    void Convert(string inputPath, string outputPath);
}
