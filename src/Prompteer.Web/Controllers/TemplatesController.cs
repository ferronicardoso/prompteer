using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Domain.Interfaces;
using Prompteer.Web.Models;
using System.Text.Json;

namespace Prompteer.Web.Controllers;

public class TemplatesController : Controller
{
    private readonly IPromptTemplateService _templateService;
    private readonly ICurrentUserService _currentUser;
    private readonly IStringLocalizer<SharedResource> _l;

    public TemplatesController(IPromptTemplateService templateService, ICurrentUserService currentUser, IStringLocalizer<SharedResource> l)
    {
        _templateService = templateService;
        _currentUser     = currentUser;
        _l               = l;
    }

    // GET /Templates
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = _l["Templates.Title"].Value;
        ViewData["Search"] = search;

        var currentUser = await _currentUser.GetCurrentUserAsync();
        var userRole = _currentUser.GetRoleFromClaims();

        var result = await _templateService.GetPagedAsync(page, 10, search, currentUser?.Id, userRole);
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
        TempData["Success"] = _l["Templates.Cloned"].Value;
        return RedirectToAction(nameof(Index));
    }

    // POST /Templates/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _templateService.DeleteAsync(id);
        TempData["Success"] = _l["Templates.Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }

    // GET /Templates/Versions/id
    public async Task<IActionResult> Versions(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return NotFound();

        var versions = await _templateService.GetVersionsAsync(id);
        ViewData["Title"] = $"{_l["Templates.Details.Versions"].Value} — {template.Name}";
        return View(new VersionsViewModel { Template = template, Versions = versions });
    }

    // GET /Templates/Version?templateId=&versionNumber=
    public async Task<IActionResult> Version(Guid templateId, int versionNumber)
    {
        var template = await _templateService.GetByIdAsync(templateId);
        if (template == null) return NotFound();

        var version = await _templateService.GetVersionAsync(templateId, versionNumber);
        if (version == null) return NotFound();

        ViewData["Title"] = $"{_l["Common.Version"].Value} {versionNumber} — {template.Name}";
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

        ViewData["Title"] = $"{_l["Templates.Details.Compare"].Value} — {template.Name}";
        return View(new CompareViewModel { Template = template, Version1 = version1, Version2 = version2 });
    }

    // ── Export ────────────────────────────────────────────────────────────────

    // POST /Templates/SetVisibility
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetVisibility(Guid id, bool isPublic)
    {
        try
        {
            await _templateService.SetVisibilityAsync(id, isPublic);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET /Templates/Export/{id}  — exporta um template
    public async Task<IActionResult> Export(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return NotFound();

        var package = await _templateService.ExportAsync(new[] { id }, _currentUser.DisplayName ?? "admin");
        var json    = JsonSerializer.Serialize(package, new JsonSerializerOptions { WriteIndented = true });
        var bytes   = System.Text.Encoding.UTF8.GetBytes(json);
        var fileName = $"prompteer-{Slugify(template.Name)}.json";
        return File(bytes, "application/json", fileName);
    }

    // GET /Templates/ExportAll  — exporta todos os templates
    public async Task<IActionResult> ExportAll()
    {
        var package  = await _templateService.ExportAsync(null, _currentUser.DisplayName ?? "admin");
        var json     = JsonSerializer.Serialize(package, new JsonSerializerOptions { WriteIndented = true });
        var bytes    = System.Text.Encoding.UTF8.GetBytes(json);
        var fileName = $"prompteer-templates-{DateTime.UtcNow:yyyyMMdd-HHmm}.json";
        return File(bytes, "application/json", fileName);
    }

    // GET /Templates/Import
    public IActionResult Import()
    {
        ViewData["Title"] = _l["Templates.Import.Title"].Value;
        return View();
    }

    // POST /Templates/Import
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile? file, bool skipDuplicates = true)
    {
        ViewData["Title"] = _l["Templates.Import.Title"].Value;

        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("", _l["Templates.Import.SelectFile"].Value);
            return View();
        }

        if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", _l["Templates.Import.JsonOnly"].Value);
            return View();
        }

        TemplateExportDto? package;
        try
        {
            using var stream = file.OpenReadStream();
            package = await JsonSerializer.DeserializeAsync<TemplateExportDto>(
                stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            ModelState.AddModelError("", _l["Templates.Import.InvalidJson"].Value);
            return View();
        }

        if (package?.Templates == null || package.Templates.Count == 0)
        {
            ModelState.AddModelError("", _l["Templates.Import.NoTemplates"].Value);
            return View();
        }

        var result = await _templateService.ImportAsync(package, skipDuplicates);
        TempData["ImportResult"] = JsonSerializer.Serialize(result);
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Slugify(string name) =>
        System.Text.RegularExpressions.Regex
            .Replace(name.ToLowerInvariant(), @"[^a-z0-9]+", "-")
            .Trim('-');
}
