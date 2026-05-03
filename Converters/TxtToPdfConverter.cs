using iText.Bouncycastle;
using FileConversionTool.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Converters;

/// <summary>
/// Converts a plain-text (.txt) file to PDF using iText 7.
///
/// Each line of the text file becomes a paragraph in the PDF.
/// Blank lines are preserved as visual spacing.
/// Font size and margins use sensible defaults (A4, 12pt, standard margins).
/// </summary>
public sealed class TxtToPdfConverter : IFileConverter
{
    private readonly ILogger<TxtToPdfConverter> _logger;

    public TxtToPdfConverter(ILogger<TxtToPdfConverter> logger) => _logger = logger;

    public string InputFormat  => "txt";
    public string OutputFormat => "pdf";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting TXT → PDF conversion: {Input} → {Output}", inputPath, outputPath);

        var lines = File.ReadAllLines(inputPath);
        _logger.LogInformation("Read {LineCount} lines from source file.", lines.Length);

        // iText7 uses a writer → PDF document → layout document layering pattern
        iText.Bouncycastleconnector.BouncyCastleFactoryCreator.SetFactory(new iText.Bouncycastle.BouncyCastleFactory());
        using var writer = new PdfWriter(outputPath);
        using var pdfDocument = new PdfDocument(writer);
        using var document    = new Document(pdfDocument);

        // Standard readable margins (in points; 72pt = 1 inch)
        document.SetMargins(72, 72, 72, 72);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                // Preserve blank lines as spacing paragraphs
                document.Add(new Paragraph("\u00A0").SetFontSize(6));
            }
            else
            {
                document.Add(
                    new Paragraph(line)
                        .SetFontSize(12)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetMultipliedLeading(1.3f)
                );
            }
        }

        _logger.LogInformation("TXT → PDF complete. Output: {Output}", outputPath);
    }
}
