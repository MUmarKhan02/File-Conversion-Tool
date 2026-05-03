using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Converters;

/// <summary>
/// Converts a CSV file to a JSON array of objects.
/// The first row of the CSV is treated as the header (property names).
/// </summary>
public sealed class CsvToJsonConverter : IFileConverter
{
    private readonly ILogger<CsvToJsonConverter> _logger;

    public CsvToJsonConverter(ILogger<CsvToJsonConverter> logger) => _logger = logger;

    public string InputFormat  => "csv";
    public string OutputFormat => "json";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting CSV → JSON conversion: {Input} → {Output}", inputPath, outputPath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Tolerate minor whitespace inconsistencies in real-world CSV files
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null // Silently handle rows with fewer fields than headers
        };

        using var reader = new StreamReader(inputPath);
        using var csv    = new CsvReader(reader, config);

        // CsvHelper reads each row as a dynamic record; we project to Dictionary for clean JSON output
        var records = new List<Dictionary<string, string>>();

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? throw new InvalidOperationException("CSV has no header row.");

        while (csv.Read())
        {
            var record = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                record[header] = csv.GetField(header) ?? string.Empty;
            }
            records.Add(record);
        }

        _logger.LogInformation("Parsed {Count} CSV records.", records.Count);

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json    = JsonSerializer.Serialize(records, options);
        File.WriteAllText(outputPath, json);

        _logger.LogInformation("CSV → JSON complete.");
    }
}
