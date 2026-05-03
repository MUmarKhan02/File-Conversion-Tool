using System.Text.Json;
using System.Xml.Linq;
using FileConversionTool.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileConversionTool.Converters;

/// <summary>
/// Converts a JSON file to XML.
///
/// Supports two JSON shapes:
///   - Array of objects: wrapped in a root <Items> element, each object becomes an <Item>
///   - Single object:    each top-level key becomes a child element of <Root>
///
/// Nested objects and arrays are recursively converted to nested XML elements.
/// XML element names are sanitised — JSON keys that start with a digit or contain
/// spaces/special chars are prefixed with an underscore to stay XML-valid.
/// </summary>
public sealed class JsonToXmlConverter : IFileConverter
{
    private readonly ILogger<JsonToXmlConverter> _logger;

    public JsonToXmlConverter(ILogger<JsonToXmlConverter> logger) => _logger = logger;

    public string InputFormat => "json";
    public string OutputFormat => "xml";

    public void Convert(string inputPath, string outputPath)
    {
        _logger.LogInformation("Starting JSON → XML conversion: {Input} → {Output}", inputPath, outputPath);

        var json = File.ReadAllText(inputPath);
        using var doc = JsonDocument.Parse(json);

        XElement root = doc.RootElement.ValueKind switch
        {
            JsonValueKind.Array => ConvertArray(doc.RootElement, "Items", "Item"),
            JsonValueKind.Object => ConvertObject(doc.RootElement, "Root"),
            _ => throw new InvalidOperationException("JSON root must be an object or array.")
        };

        var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        xdoc.Save(outputPath);

        _logger.LogInformation("JSON → XML complete. Output: {Output}", outputPath);
    }

    private static XElement ConvertObject(JsonElement obj, string elementName)
    {
        var el = new XElement(Sanitise(elementName));
        foreach (var prop in obj.EnumerateObject())
        {
            el.Add(ConvertElement(prop.Value, prop.Name));
        }
        return el;
    }

    private static XElement ConvertArray(JsonElement arr, string wrapperName, string itemName)
    {
        var wrapper = new XElement(Sanitise(wrapperName));
        foreach (var item in arr.EnumerateArray())
        {
            wrapper.Add(ConvertElement(item, itemName));
        }
        return wrapper;
    }

    private static XElement ConvertElement(JsonElement el, string name)
    {
        var safeName = Sanitise(name);
        return el.ValueKind switch
        {
            JsonValueKind.Object => ConvertObject(el, safeName),
            JsonValueKind.Array => ConvertArray(el, safeName, "Item"),
            JsonValueKind.Null => new XElement(safeName),
            _ => new XElement(safeName, el.ToString())
        };
    }

    /// <summary>
    /// Ensures the name is a valid XML element name.
    /// Replaces spaces with underscores and prefixes names starting with a digit.
    /// </summary>
    private static string Sanitise(string name)
    {
        name = name.Replace(" ", "_");
        if (name.Length > 0 && (char.IsDigit(name[0]) || name[0] == '-'))
            name = "_" + name;
        return name;
    }
}