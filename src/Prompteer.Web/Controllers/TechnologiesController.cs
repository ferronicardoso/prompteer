using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Domain.Enums;

namespace Prompteer.Web.Controllers;

public class TechnologiesController : Controller
{
    private readonly ITechnologyService _service;
    private readonly IStringLocalizer<SharedResource> _l;

    public TechnologiesController(ITechnologyService service, IStringLocalizer<SharedResource> l)
    {
        _service = service;
        _l = l;
    }

    private void PopulateCategoryList(TechCategory? selected = null)
    {
        ViewBag.CategoryItems = new SelectList(new[]
        {
            new { V = (int)TechCategory.Framework,             T = "Framework" },
            new { V = (int)TechCategory.Database,               T = _l["TechCategory.Database"].Value },
            new { V = (int)TechCategory.ORM,                    T = "ORM" },
            new { V = (int)TechCategory.Frontend,               T = "Frontend" },
            new { V = (int)TechCategory.Auth,                   T = _l["TechCategory.Auth"].Value },
            new { V = (int)TechCategory.Messaging,              T = _l["TechCategory.Messaging"].Value },
            new { V = (int)TechCategory.Cache,                  T = "Cache" },
            new { V = (int)TechCategory.Observability,          T = _l["TechCategory.Observability"].Value },
            new { V = (int)TechCategory.DevOps,                 T = "DevOps" },
            new { V = (int)TechCategory.Testing,                T = _l["TechCategory.Testing"].Value },
            new { V = (int)TechCategory.ArtificialIntelligence, T = _l["TechCategory.ArtificialIntelligence"].Value },
            new { V = (int)TechCategory.Other,                  T = _l["TechCategory.Other"].Value },
        }, "V", "T", selected.HasValue ? (int?)selected.Value : null);
    }

    private void PopulateEcosystemList(TechEcosystem? selected = null)
    {
        ViewBag.EcosystemItems = new SelectList(new[]
        {
            new { V = (int)TechEcosystem.DotNet,   T = ".NET" },
            new { V = (int)TechEcosystem.Node,     T = "Node.js" },
            new { V = (int)TechEcosystem.Python,   T = "Python" },
            new { V = (int)TechEcosystem.Java,     T = "Java" },
            new { V = (int)TechEcosystem.Agnostic, T = _l["TechEcosystem.Agnostic"].Value },
        }, "V", "T", selected.HasValue ? (int?)selected.Value : null);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, TechCategory? category = null, TechEcosystem? ecosystem = null)
    {
        ViewData["Title"] = _l["Technologies.Title"].Value;
        ViewData["Search"] = search;
        ViewData["Category"] = category;
        ViewData["Ecosystem"] = ecosystem;
        PopulateCategoryList(category);
        PopulateEcosystemList(ecosystem);
        var result = await _service.GetPagedAsync(page, 10, search, category, ecosystem);
        return View(result);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = _l["Technologies.Create.Title"].Value;
        PopulateCategoryList();
        PopulateEcosystemList();
        return View(new TechnologyFormDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TechnologyFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            PopulateCategoryList(dto.Category);
            PopulateEcosystemList(dto.Ecosystem);
            return View(dto);
        }
        await _service.CreateAsync(dto);
        TempData["Success"] = _l["Technologies.Created"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tech = await _service.GetByIdAsync(id);
        if (tech == null) return NotFound();
        if (tech.IsSystemDefault)
        {
            TempData["Error"] = _l["Technologies.CannotEdit"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["Technologies.Edit.Title"].Value;
        PopulateCategoryList(tech.Category);
        PopulateEcosystemList(tech.Ecosystem);
        var form = new TechnologyFormDto
        {
            Id = tech.Id,
            Name = tech.Name,
            Category = tech.Category,
            Ecosystem = tech.Ecosystem,
            Version = tech.Version,
            ShortDescription = tech.ShortDescription
        };
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TechnologyFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            PopulateCategoryList(dto.Category);
            PopulateEcosystemList(dto.Ecosystem);
            return View(dto);
        }
        try
        {
            await _service.UpdateAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        TempData["Success"] = _l["Technologies.Updated"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(Guid id)
    {
        var clone = await _service.CloneAsync(id);
        TempData["Success"] = string.Format(_l["Technologies.ClonedAs"].Value, clone.Name);
        return RedirectToAction(nameof(Edit), new { id = clone.Id });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var tech = await _service.GetByIdAsync(id);
        if (tech == null) return NotFound();
        if (tech.IsSystemDefault)
        {
            TempData["Error"] = _l["Technologies.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["Technologies.Delete.Title"].Value;
        return View(tech);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var tech = await _service.GetByIdAsync(id);
        if (tech?.IsSystemDefault == true)
        {
            TempData["Error"] = _l["Technologies.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        await _service.DeleteAsync(id);
        TempData["Success"] = _l["Technologies.Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }
}
