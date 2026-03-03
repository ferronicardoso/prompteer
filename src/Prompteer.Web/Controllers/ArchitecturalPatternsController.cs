using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Domain.Enums;

namespace Prompteer.Web.Controllers;

public class ArchitecturalPatternsController : Controller
{
    private readonly IArchitecturalPatternService _service;
    private readonly IStringLocalizer<SharedResource> _l;

    public ArchitecturalPatternsController(IArchitecturalPatternService service, IStringLocalizer<SharedResource> l)
    {
        _service = service;
        _l = l;
    }

    private void PopulateEcosystemList(TechEcosystem selected = TechEcosystem.Agnostic)
    {
        ViewBag.EcosystemItems = new SelectList(new[]
        {
            new { V = (int)TechEcosystem.DotNet,   T = ".NET" },
            new { V = (int)TechEcosystem.Node,     T = "Node.js" },
            new { V = (int)TechEcosystem.Python,   T = "Python" },
            new { V = (int)TechEcosystem.Java,     T = "Java" },
            new { V = (int)TechEcosystem.Agnostic, T = _l["TechEcosystem.Agnostic"].Value },
        }, "V", "T", (int)selected);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = _l["ArchitecturalPatterns.Title"].Value;
        ViewData["Search"] = search;
        var result = await _service.GetPagedAsync(page, 10, search);
        return View(result);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = _l["ArchitecturalPatterns.Create.Title"].Value;
        PopulateEcosystemList();
        return View(new ArchitecturalPatternFormDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArchitecturalPatternFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            PopulateEcosystemList(dto.Ecosystem);
            return View(dto);
        }
        await _service.CreateAsync(dto);
        TempData["Success"] = _l["ArchitecturalPatterns.Created"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var pattern = await _service.GetByIdAsync(id);
        if (pattern == null) return NotFound();
        if (pattern.IsSystemDefault)
        {
            TempData["Error"] = _l["ArchitecturalPatterns.CannotEdit"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["ArchitecturalPatterns.Edit.Title"].Value;
        PopulateEcosystemList(pattern.Ecosystem);
        var form = new ArchitecturalPatternFormDto
        {
            Id = pattern.Id,
            Name = pattern.Name,
            Description = pattern.Description,
            Ecosystem = pattern.Ecosystem
        };
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ArchitecturalPatternFormDto dto)
    {
        if (!ModelState.IsValid)
        {
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
        TempData["Success"] = _l["ArchitecturalPatterns.Updated"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(Guid id)
    {
        var clone = await _service.CloneAsync(id);
        TempData["Success"] = string.Format(_l["ArchitecturalPatterns.ClonedAs"].Value, clone.Name);
        return RedirectToAction(nameof(Edit), new { id = clone.Id });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var pattern = await _service.GetByIdAsync(id);
        if (pattern == null) return NotFound();
        if (pattern.IsSystemDefault)
        {
            TempData["Error"] = _l["ArchitecturalPatterns.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["ArchitecturalPatterns.Delete.Title"].Value;
        return View(pattern);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var pattern = await _service.GetByIdAsync(id);
        if (pattern?.IsSystemDefault == true)
        {
            TempData["Error"] = _l["ArchitecturalPatterns.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        await _service.DeleteAsync(id);
        TempData["Success"] = _l["ArchitecturalPatterns.Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }
}
