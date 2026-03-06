using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Web.Filters;

namespace Prompteer.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class SettingsController(
    IAppSettingService settings,
    IAIService aiService,
    IStringLocalizer<SharedResource> localizer,
    IMemoryCache cache) : Controller
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
        ViewData["LogoUrl"] = all.GetValueOrDefault("App:LogoUrl", "");
        ViewData["AzureAd:TenantId"] = all.GetValueOrDefault("AzureAd:TenantId", "");
        ViewData["AzureAd:ClientId"] = all.GetValueOrDefault("AzureAd:ClientId", "");
        ViewData["AzureAd:Domain"]   = all.GetValueOrDefault("AzureAd:Domain",   "");

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
            return Json(new { success = false, error = localizer["Settings.ErrorApiKeyRequired"].Value });

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
    public async Task<IActionResult> SaveEntra(string? tenantId, string? clientId, string? clientSecret, string? domain)
    {
        tenantId     = tenantId?.Trim();
        clientId     = clientId?.Trim();
        clientSecret = clientSecret?.Trim();
        domain       = domain?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
            return RedirectToAction(nameof(Index), new { tab = "entra", error = localizer["Settings.ErrorEntraRequired"].Value });

        try
        {
            var dict = new Dictionary<string, string>
            {
                ["AzureAd:TenantId"] = tenantId,
                ["AzureAd:ClientId"] = clientId,
                ["AzureAd:Domain"]   = domain
            };
            if (!string.IsNullOrWhiteSpace(clientSecret))
                dict["AzureAd:ClientSecret"] = clientSecret;

            await settings.SaveManyAsync(dict);
            return RedirectToAction(nameof(Index), new { tab = "entra", saved = "entra" });
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(Index), new { tab = "entra", error = Uri.EscapeDataString(string.Format(localizer["Settings.ErrorSaveFailed"].Value, ex.Message)) });
        }
    }

    // POST /Settings/SaveGeneral
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGeneral(string? language, string? timeZone, string? dateFormat, string? logoUrl)
    {
        language   = language?.Trim()   ?? "en";
        timeZone   = timeZone?.Trim()   ?? "UTC";
        dateFormat = dateFormat?.Trim() ?? "MM/dd/yyyy";
        logoUrl    = logoUrl?.Trim()    ?? "";

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

        // Validate logo URL: must be empty or a well-formed absolute http/https URL
        if (!string.IsNullOrEmpty(logoUrl) &&
            !(Uri.TryCreate(logoUrl, UriKind.Absolute, out var logoUri) &&
              (logoUri.Scheme == Uri.UriSchemeHttp || logoUri.Scheme == Uri.UriSchemeHttps)))
        {
            logoUrl = "";
        }

        await settings.SaveManyAsync(new Dictionary<string, string>
        {
            ["App:Language"]   = language,
            ["App:TimeZone"]   = timeZone,
            ["App:DateFormat"] = dateFormat,
            ["App:LogoUrl"]    = logoUrl
        });

        cache.Remove(AppSettingsViewDataFilter.CacheKey);

        return RedirectToAction(nameof(Index), new { tab = "general", saved = "general" });
    }
}
