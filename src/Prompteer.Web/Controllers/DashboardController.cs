using Microsoft.AspNetCore.Mvc;
using Prompteer.Application.Services;

namespace Prompteer.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IPromptTemplateService _templateService;

    public DashboardController(IPromptTemplateService templateService)
    {
        _templateService = templateService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        var stats = await _templateService.GetDashboardStatsAsync();
        return View(stats);
    }
}
