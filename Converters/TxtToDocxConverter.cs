using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Converters;

/// <summary>
/// Converts a plain-text (.txt) file to a Word document (.docx).
///
/// Uses DocumentFormat.OpenXml (the official Microsoft OpenXML SDK).
/// Each line in the text file becomes a Paragraph in the Word document.
/// Blank lines are preserved as empty paragraphs for visual spacing.
/// Font is set to Calibri 11pt to match Word's default "Normal" style.
/// </summary>
public sealed class TxtToDocxConverter : IFileConverter
{
    private readonly ILogger<TxtToDocxConverter> _logger;

    public TxtToDocxConverter(ILogger<TxtToDocxConverter> logger) => _logger = logger;

    public string InputFormat => "txt";
    public string OutputFormat => "docx";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting TXT → DOCX conversion: {Input} → {Output}", inputPath, outputPath);

        var lines = File.ReadAllLines(inputPath);
        _logger.LogInformation("Read {Count} lines.", lines.Length);

        // WordprocessingDocument creates the .docx package (ZIP of XML files under the hood)
        using var doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Add document-wide default styles (Calibri 11pt, matching Word's "Normal")
        AddDefaultStyles(mainPart);

        foreach (var line in lines)
        {
            var para = new Paragraph();
            var run = new Run();

            // Apply font and size via RunProperties on every run
            run.RunProperties = new RunProperties(
                new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" },
                new FontSize { Val = "22" }  // OpenXML font size is in half-points: 22 = 11pt
            );

            run.AppendChild(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
            para.AppendChild(run);
            body.AppendChild(para);
        }

        // Word requires a final section properties element at the end of the body
        body.AppendChild(new SectionProperties());

        mainPart.Document.Save();
        _logger.LogInformation("TXT → DOCX complete. Output: {Output}", outputPath);
    }

    /// <summary>
    /// Injects a minimal Styles part so Word doesn't complain about a missing style sheet.
    /// Without this, some Word versions show a repair warning on open.
    /// </summary>
    private static void AddDefaultStyles(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles(
            new DocDefaults(
                new RunPropertiesDefault(
                    new RunPropertiesBaseStyle(
                        new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" },
                        new FontSize { Val = "22" }
                    )
                )
            )
        );
        stylesPart.Styles.Save();
    }
}