using Microsoft.AspNetCore.Mvc;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Web.Helpers;

namespace Prompteer.Web.Controllers;

public class SettingsController(
    IAppSettingService settings,
    IAIService aiService,
    IWebHostEnvironment env) : Controller
{
    // GET /Settings  or  /Settings?tab=entra&saved=entra  or  &error=msg
    public async Task<IActionResult> Index(string? tab, string? saved, string? error)
    {
        ViewData["Title"] = "Settings";
        ViewData["ActiveTab"]  = tab   ?? "ai";
        ViewData["SavedScope"] = saved ?? "";
        ViewData["ErrorMsg"]   = error ?? "";

        var all = await settings.GetAllAsync();
        var dto = new AISettingsDto
        {
            Provider = all.GetValueOrDefault("AI:Provider", "OpenAI"),
            ApiKey   = all.GetValueOrDefault("AI:ApiKey",   ""),
            Model    = all.GetValueOrDefault("AI:Model",    "")
        };

        ViewData["DefaultLanguage"] = all.GetValueOrDefault("App:Language",   "en");
        ViewData["DefaultTimeZone"] = all.GetValueOrDefault("App:TimeZone",   "UTC");
        ViewData["DefaultDateFormat"] = all.GetValueOrDefault("App:DateFormat", "MM/dd/yyyy");

        return View(dto);
    }

    // POST /Settings/Index  (AI settings)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AISettingsDto dto)
    {
        await settings.SaveManyAsync(new Dictionary<string, string>
        {
            ["AI:Provider"] = string.IsNullOrWhiteSpace(dto.Provider) ? "OpenAI" : dto.Provider.Trim(),
            ["AI:ApiKey"]   = dto.ApiKey?.Trim() ?? "",
            ["AI:Model"]    = dto.Model?.Trim()  ?? ""
        });
        return RedirectToAction(nameof(Index), new { tab = "ai", saved = "ai" });
    }

    // AJAX: GET /Settings/GetModels?provider=OpenAI&apiKey=sk-...
    [HttpGet]
    public async Task<IActionResult> GetModels(string provider, string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = await settings.GetAsync("AI:ApiKey");

        if (string.IsNullOrWhiteSpace(apiKey))
            return Json(new { success = false, error = "Enter the API Key before fetching models." });

        try
        {
            var models = await aiService.GetModelsAsync(provider ?? "OpenAI", apiKey);
            return Json(new { success = true, models });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // POST /Settings/SaveEntra
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveEntra(string? tenantId, string? clientId, string? clientSecret, string? domain)
    {
        tenantId     = tenantId?.Trim();
        clientId     = clientId?.Trim();
        clientSecret = clientSecret?.Trim();
        domain       = domain?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
            return RedirectToAction(nameof(Index), new { tab = "entra", error = "TenantId and ClientId are required." });

        try
        {
            var path = Path.Combine(env.ContentRootPath, "appsettings.json");
            AppSettingsWriter.WriteAzureAd(path, tenantId, clientId, clientSecret, domain);
            return RedirectToAction(nameof(Index), new { tab = "entra", saved = "entra" });
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Index), new { tab = "entra", error = Uri.EscapeDataString($"Failed to save: {ex.Message}") });
        }
    }

    // POST /Settings/SaveGeneral
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGeneral(string? language, string? timeZone, string? dateFormat)
    {
        language   = language?.Trim()   ?? "en";
        timeZone   = timeZone?.Trim()   ?? "UTC";
        dateFormat = dateFormat?.Trim() ?? "MM/dd/yyyy";

        // Validate language
        if (language != "en" && language != "pt-BR")
            language = "en";

        // Validate timezone
        try { TimeZoneInfo.FindSystemTimeZoneById(timeZone); }
        catch { timeZone = "UTC"; }

        // Validate date format (whitelist)
        var validFormats = new[] { "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd MMM yyyy" };
        if (!validFormats.Contains(dateFormat))
            dateFormat = "MM/dd/yyyy";

        await settings.SaveManyAsync(new Dictionary<string, string>
        {
            ["App:Language"]   = language,
            ["App:TimeZone"]   = timeZone,
            ["App:DateFormat"] = dateFormat
        });

        return RedirectToAction(nameof(Index), new { tab = "general", saved = "general" });
    }
}
