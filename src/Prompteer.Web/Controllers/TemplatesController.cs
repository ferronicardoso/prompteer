using Microsoft.AspNetCore.Mvc;
using Prompteer.Application.Services;
using Prompteer.Web.Models;

namespace Prompteer.Web.Controllers;

public class TemplatesController : Controller
{
    private readonly IPromptTemplateService _templateService;

    public TemplatesController(IPromptTemplateService templateService)
    {
        _templateService = templateService;
    }

    // GET /Templates
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = "Templates de Prompt";
        ViewData["Search"] = search;
        var result = await _templateService.GetPagedAsync(page, 10, search);
        return View(result);
    }

    // GET /Templates/Details/id
    public async Task<IActionResult> Details(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return NotFound();

        var versions = await _templateService.GetVersionsAsync(id);
        var latest = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

        ViewData["Title"] = template.Name;
        return View(new TemplateDetailsViewModel
        {
            Template = template,
            Versions = versions,
            LatestPrompt = latest?.GeneratedPrompt
        });
    }

    // GET /Templates/Edit/id — redirect to wizard
    public IActionResult Edit(Guid id)
    {
        return RedirectToAction("Index", "PromptGenerator", new { templateId = id });
    }

    // POST /Templates/Clone
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(Guid id, string newName)
    {
        await _templateService.CloneAsync(id, newName);
        TempData["Success"] = "Template clonado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // POST /Templates/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _templateService.DeleteAsync(id);
        TempData["Success"] = "Template excluído com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // GET /Templates/Versions/id
    public async Task<IActionResult> Versions(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return NotFound();

        var versions = await _templateService.GetVersionsAsync(id);
        ViewData["Title"] = $"Versões — {template.Name}";
        return View(new VersionsViewModel { Template = template, Versions = versions });
    }

    // GET /Templates/Version?templateId=&versionNumber=
    public async Task<IActionResult> Version(Guid templateId, int versionNumber)
    {
        var template = await _templateService.GetByIdAsync(templateId);
        if (template == null) return NotFound();

        var version = await _templateService.GetVersionAsync(templateId, versionNumber);
        if (version == null) return NotFound();

        ViewData["Title"] = $"Versão {versionNumber} — {template.Name}";
        return View(new VersionDetailViewModel { Template = template, Version = version });
    }

    // GET /Templates/Compare?templateId=&v1=&v2=
    public async Task<IActionResult> Compare(Guid templateId, int v1, int v2)
    {
        var template = await _templateService.GetByIdAsync(templateId);
        if (template == null) return NotFound();

        var version1 = await _templateService.GetVersionAsync(templateId, v1);
        var version2 = await _templateService.GetVersionAsync(templateId, v2);
        if (version1 == null || version2 == null) return NotFound();

        ViewData["Title"] = $"Comparar versões — {template.Name}";
        return View(new CompareViewModel { Template = template, Version1 = version1, Version2 = version2 });
    }
}
