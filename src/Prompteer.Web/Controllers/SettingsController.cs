using Microsoft.AspNetCore.Mvc;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;

namespace Prompteer.Web.Controllers;

public class SettingsController(IAppSettingService settings, IAIService aiService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Configurações";
        var all = await settings.GetAllAsync();
        var dto = new AISettingsDto
        {
            Provider = all.GetValueOrDefault("AI:Provider", "OpenAI"),
            ApiKey   = all.GetValueOrDefault("AI:ApiKey", ""),
            Model    = all.GetValueOrDefault("AI:Model", "")
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AISettingsDto dto)
    {
        ViewData["Title"] = "Configurações";
        await settings.SaveManyAsync(new Dictionary<string, string>
        {
            ["AI:Provider"] = string.IsNullOrWhiteSpace(dto.Provider) ? "OpenAI" : dto.Provider.Trim(),
            ["AI:ApiKey"]   = dto.ApiKey?.Trim() ?? "",
            ["AI:Model"]    = dto.Model?.Trim() ?? ""
        });
        TempData["Success"] = "Configurações salvas com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // AJAX: GET /Settings/GetModels?provider=OpenAI&apiKey=sk-...
    [HttpGet]
    public async Task<IActionResult> GetModels(string provider, string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = await settings.GetAsync("AI:ApiKey");

        if (string.IsNullOrWhiteSpace(apiKey))
            return Json(new { success = false, error = "Informe a API Key antes de buscar os modelos." });

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
}
