using FileConversionTool.Converters;
using FileConversionTool.Interfaces;
using FileConversionTool.Services;

// ─── Composition Root ─────────────────────────────────────────────────────────
// Same DI wiring as the console version — converters, factory, service.
// The only additions are ASP.NET Core MVC and static file serving.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();  // needed for TempData to survive the POST→redirect→GET

// Register converters — adding a new one here is all that's needed
builder.Services.AddSingleton<IFileConverter, JsonToCsvConverter>();
builder.Services.AddSingleton<IFileConverter, CsvToJsonConverter>();
builder.Services.AddSingleton<IFileConverter, TxtToPdfConverter>();
builder.Services.AddSingleton<IFileConverter, TxtToDocxConverter>();
builder.Services.AddSingleton<IFileConverter, JsonToXmlConverter>();
builder.Services.AddSingleton<IFileConverter, XmlToJsonConverter>();
builder.Services.AddSingleton<IFileConverter, JpgToPngConverter>();
builder.Services.AddSingleton<IFileConverter, PngToJpgConverter>();
builder.Services.AddSingleton<IFileConverter, PngToWebpConverter>();
builder.Services.AddSingleton<IFileConverter, BmpToPngConverter>();

builder.Services.AddSingleton<ConverterFactory>();
builder.Services.AddSingleton<ConversionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();