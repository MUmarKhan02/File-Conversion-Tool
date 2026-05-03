using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace FileConversionTool.Converters;

/// <summary>
/// PNG → WEBP. Uses lossless encoding to preserve the PNG's full quality.
/// WebP lossless is typically 25-35% smaller than an equivalent PNG.
/// </summary>
public sealed class PngToWebpConverter : IFileConverter
{
    private readonly ILogger<PngToWebpConverter> _logger;
    public PngToWebpConverter(ILogger<PngToWebpConverter> logger) => _logger = logger;

    public string InputFormat => "png";
    public string OutputFormat => "webp";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting PNG → WEBP: {Input} → {Output}", inputPath, outputPath);
        using var image = Image.Load(inputPath);
        image.Save(outputPath, new WebpEncoder { FileFormat = WebpFileFormatType.Lossless });
        _logger.LogInformation("PNG → WEBP complete.");
    }
}