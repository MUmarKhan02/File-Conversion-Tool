using System.Globalization;
using System.Text.Json;
using CsvHelper;
using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Converters;

/// <summary>
/// Converts a JSON file (array of flat objects) to CSV format.
///
/// Assumptions:
///   - Input JSON is an array of objects (e.g. [{"name":"Alice","age":30}, ...])
///   - All objects share the same keys (used as CSV headers)
///   - Values are primitives (strings, numbers, booleans)
/// </summary>
public sealed class JsonToCsvConverter : IFileConverter
{
    private readonly ILogger<JsonToCsvConverter> _logger;

    public JsonToCsvConverter(ILogger<JsonToCsvConverter> logger) => _logger = logger;

    public string InputFormat  => "json";
    public string OutputFormat => "csv";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting JSON → CSV conversion: {Input} → {Output}", inputPath, outputPath);

        var json = File.ReadAllText(inputPath);

        // Deserialize into a list of dictionaries — works for any flat JSON object schema
        var records = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json)
            ?? throw new InvalidOperationException("JSON file is empty or not a JSON array.");

        if (records.Count == 0)
            throw new InvalidOperationException("JSON array contains no records.");

        using var writer = new StreamWriter(outputPath);
        using var csv    = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write header row from the keys of the first object
        var headers = records[0].Keys.ToList();
        foreach (var header in headers)
        {
            csv.WriteField(header);
        }
        csv.NextRecord();

        // Write data rows
        foreach (var record in records)
        {
            foreach (var header in headers)
            {
                var value = record.TryGetValue(header, out var element)
                    ? element.ToString()
                    : string.Empty;
                csv.WriteField(value);
            }
            csv.NextRecord();
        }

        _logger.LogInformation("JSON → CSV complete. {Count} records written.", records.Count);
    }
}
