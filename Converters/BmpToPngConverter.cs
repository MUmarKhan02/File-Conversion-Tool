using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace FileConversionTool.Converters;

public sealed class BmpToPngConverter : IFileConverter
{
    private readonly ILogger<BmpToPngConverter> _logger;
    public BmpToPngConverter(ILogger<BmpToPngConverter> logger) => _logger = logger;

    public string InputFormat => "bmp";
    public string OutputFormat => "png";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting BMP → PNG: {Input} → {Output}", inputPath, outputPath);
        using var image = Image.Load(inputPath);
        image.Save(outputPath, new PngEncoder());
        _logger.LogInformation("BMP → PNG complete.");
    }
}