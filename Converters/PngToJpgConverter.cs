using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace FileConversionTool.Converters;

/// <summary>
/// PNG → JPG. Quality is set to 90 (out of 100) — good balance between
/// file size and visual fidelity. JPG does not support transparency;
/// any transparent pixels in the PNG are composited against white.
/// </summary>
public sealed class PngToJpgConverter : IFileConverter
{
    private readonly ILogger<PngToJpgConverter> _logger;
    public PngToJpgConverter(ILogger<PngToJpgConverter> logger) => _logger = logger;

    public string InputFormat => "png";
    public string OutputFormat => "jpg";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting PNG → JPG: {Input} → {Output}", inputPath, outputPath);
        using var image = Image.Load(inputPath);
        image.Save(outputPath, new JpegEncoder { Quality = 90 });
        _logger.LogInformation("PNG → JPG complete.");
    }
}