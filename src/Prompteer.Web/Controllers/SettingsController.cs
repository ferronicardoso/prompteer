using Microsoft.AspNetCore.Mvc;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;

namespace Prompteer.Web.Controllers;

public class SettingsController(IAppSettingService settings) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Configurações";
        var all = await settings.GetAllAsync();
        var dto = new AISettingsDto
        {
            ApiKey = all.GetValueOrDefault("AI:ApiKey", ""),
            Model  = all.GetValueOrDefault("AI:Model", "gpt-4o-mini")
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
            ["AI:ApiKey"] = dto.ApiKey?.Trim() ?? "",
            ["AI:Model"]  = string.IsNullOrWhiteSpace(dto.Model) ? "gpt-4o-mini" : dto.Model.Trim()
        });
        TempData["Success"] = "Configurações salvas com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
