using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace FileConversionTool.Converters;

public sealed class JpgToPngConverter : IFileConverter
{
    private readonly ILogger<JpgToPngConverter> _logger;
    public JpgToPngConverter(ILogger<JpgToPngConverter> logger) => _logger = logger;

    public string InputFormat => "jpg";
    public string OutputFormat => "png";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting JPG → PNG: {Input} → {Output}", inputPath, outputPath);
        using var image = Image.Load(inputPath);
        image.Save(outputPath, new PngEncoder());
        _logger.LogInformation("JPG → PNG complete.");
    }
}