using System.Text.Json;
using System.Xml.Linq;
using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Converters;

/// <summary>
/// Converts an XML file to JSON.
///
/// The XML root element is used as the top-level JSON object key.
/// Child elements with the same tag name under a parent are collapsed
/// into a JSON array automatically (e.g. multiple <Item> → [...]).
/// Attributes are included as properties prefixed with "@" (e.g. "@id").
/// Text-only elements become string values directly.
/// </summary>
public sealed class XmlToJsonConverter : IFileConverter
{
    private readonly ILogger<XmlToJsonConverter> _logger;

    public XmlToJsonConverter(ILogger<XmlToJsonConverter> logger) => _logger = logger;

    public string InputFormat => "xml";
    public string OutputFormat => "json";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting XML → JSON conversion: {Input} → {Output}", inputPath, outputPath);

        var xdoc = XDocument.Load(inputPath);
        if (xdoc.Root is null)
            throw new InvalidOperationException("XML file has no root element.");

        var result = new Dictionary<string, object?>
        {
            [xdoc.Root.Name.LocalName] = ConvertElement(xdoc.Root)
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(result, options);
        File.WriteAllText(outputPath, json);

        _logger.LogInformation("XML → JSON complete. Output: {Output}", outputPath);
    }

    private static object? ConvertElement(XElement el)
    {
        var children = el.Elements().ToList();
        var attributes = el.Attributes().ToList();
        var text = el.Value.Trim();

        // Pure text node with no children or attributes → just a string value
        if (children.Count == 0 && attributes.Count == 0)
            return string.IsNullOrEmpty(text) ? null : (object)text;

        var dict = new Dictionary<string, object?>();

        // Add attributes prefixed with "@"
        foreach (var attr in attributes)
            dict["@" + attr.Name.LocalName] = attr.Value;

        // Group children by tag name — repeated tags become arrays
        var groups = children.GroupBy(c => c.Name.LocalName);
        foreach (var group in groups)
        {
            var items = group.Select(ConvertElement).ToList();
            dict[group.Key] = items.Count == 1 ? items[0] : (object)items;
        }

        // If the element has both text and children, include text under "#text"
        if (children.Count > 0 && !string.IsNullOrEmpty(text))
            dict["#text"] = text;

        return dict;
    }
}