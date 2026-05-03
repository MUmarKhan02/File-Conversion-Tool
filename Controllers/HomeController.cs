using FileConversionTool.Models;
using FileConversionTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileConversionTool.Controllers;

/// <summary>
/// Uses the Post/Redirect/Get (PRG) pattern throughout:
///   POST /Home/Convert  → does the work, stores result in TempData, redirects
///   GET  /             → reads TempData to show result (survives one load only)
///
/// This means refreshing the page after a conversion always shows a clean form —
/// TempData is consumed on the first GET and never rendered again.
/// </summary>
public class HomeController : Controller
{
    private readonly ConversionService _conversionService;
    private readonly ConverterFactory _factory;
    private readonly ILogger<HomeController> _logger;
    private readonly string _tempDir;

    public HomeController(
        ConversionService conversionService,
        ConverterFactory factory,
        ILogger<HomeController> logger)
    {
        _conversionService = conversionService;
        _factory = factory;
        _logger = logger;

        _tempDir = Path.Combine(Path.GetTempPath(), "FileConversionTool");
        Directory.CreateDirectory(_tempDir);
    }

    // GET / — reads one-shot TempData result if present, then it's gone
    public IActionResult Index()
    {
        PopulateSupportedConversions();

        var vm = new ConversionViewModel();

        if (TempData["Converted"] is true or "True")
        {
            vm.Converted = true;
            vm.Success = TempData["Success"] is true or "True";
            vm.Message = TempData["Message"] as string;
            vm.DownloadToken = TempData["DownloadToken"] as string;
            vm.OriginalFileName = TempData["OriginalFileName"] as string;

            // Keep the file token alive so Download can still access it
            // TempData.Keep() re-marks it for the next request instead of expiring it
            if (vm.DownloadToken is not null)
                TempData.Keep(vm.DownloadToken);
        }

        return View(vm);
    }

    // POST /Home/Convert
    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Convert(ConversionViewModel vm)
    {
        if (vm.UploadedFile is null || vm.UploadedFile.Length == 0)
        {
            TempData["Converted"] = true;
            TempData["Success"] = false;
            TempData["Message"] = "Please select a file to upload.";
            return RedirectToAction(nameof(Index));
        }

        var inputExt = Path.GetExtension(vm.UploadedFile.FileName).TrimStart('.');
        var inputPath = Path.Combine(_tempDir, $"{Guid.NewGuid()}.{inputExt}");

        await using (var stream = System.IO.File.Create(inputPath))
            await vm.UploadedFile.CopyToAsync(stream);

        var outputPath = Path.ChangeExtension(inputPath, vm.OutputFormat);
        var request = new ConversionRequest(inputPath, outputPath, vm.InputFormat, vm.OutputFormat);
        var result = _conversionService.Convert(request);

        if (System.IO.File.Exists(inputPath))
            System.IO.File.Delete(inputPath);

        TempData["Converted"] = true;
        TempData["Success"] = result.Success;
        TempData["Message"] = result.Message;

        if (result.Success)
        {
            var token = Guid.NewGuid().ToString("N");
            TempData[token] = outputPath;
            TempData["DownloadToken"] = token;
            TempData["OriginalFileName"] = Path.GetFileNameWithoutExtension(vm.UploadedFile.FileName)
                                             + "." + vm.OutputFormat;
        }

        // PRG: redirect to GET so the browser URL is clean — refresh won't resubmit
        return RedirectToAction(nameof(Index));
    }

    // GET /Home/Download?token=...&fileName=...
    public IActionResult Download(string token, string fileName)
    {
        if (TempData[token] is not string filePath || !System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Download token expired or not found: {Token}", token);
            return NotFound("Download link has expired. Please convert the file again.");
        }

        var ext = Path.GetExtension(filePath).TrimStart('.').ToLower();
        var contentType = ext switch
        {
            "csv" => "text/csv",
            "json" => "application/json",
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xml" => "application/xml",
            "png" => "image/png",
            "jpg" => "image/jpeg",
            "webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var bytes = System.IO.File.ReadAllBytes(filePath);
        System.IO.File.Delete(filePath);

        return File(bytes, contentType, fileName);
    }

    private void PopulateSupportedConversions()
    {
        ViewBag.Conversions = _factory
            .GetSupportedConversions()
            .Select(c => new { Input = c.Input, Output = c.Output })
            .ToList();
    }
}