namespace FileConversionTool.Models;

/// <summary>
/// ViewModel passed between the controller and Razor views.
/// Keeps web-layer concerns (IFormFile, error display) out of the core models.
/// </summary>
public class ConversionViewModel
{
    /// <summary>The uploaded file from the drag-and-drop / browse input.</summary>
    public IFormFile? UploadedFile { get; set; }

    /// <summary>e.g. "json", "csv", "txt"</summary>
    public string InputFormat { get; set; } = string.Empty;

    /// <summary>e.g. "csv", "json", "pdf"</summary>
    public string OutputFormat { get; set; } = string.Empty;

    // ── Result state (populated after conversion) ──
    public bool Converted { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }

    /// <summary>Temp filename used to serve the download.</summary>
    public string? DownloadToken { get; set; }
    public string? OriginalFileName { get; set; }
}
