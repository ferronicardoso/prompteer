using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;

namespace Prompteer.Web.Controllers;

public class BacklogToolsController : Controller
{
    private readonly IBacklogToolService _service;
    private readonly IStringLocalizer<SharedResource> _l;

    public BacklogToolsController(IBacklogToolService service, IStringLocalizer<SharedResource> l)
    {
        _service = service;
        _l = l;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = _l["BacklogTools.Title"].Value;
        ViewData["Search"] = search;
        var result = await _service.GetPagedAsync(page, 10, search);
        return View(result);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = _l["BacklogTools.Create.Title"].Value;
        return View(new BacklogToolFormDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BacklogToolFormDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = _l["BacklogTools.Created"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tool = await _service.GetByIdAsync(id);
        if (tool == null) return NotFound();
        if (tool.IsSystemDefault)
        {
            TempData["Error"] = _l["BacklogTools.CannotEdit"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["BacklogTools.Edit.Title"].Value;
        var form = new BacklogToolFormDto
        {
            Id = tool.Id,
            Name = tool.Name,
            DefaultInstructions = tool.DefaultInstructions
        };
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BacklogToolFormDto dto)
    {
        if (dto.Id.HasValue)
        {
            var existing = await _service.GetByIdAsync(dto.Id.Value);
            if (existing?.IsSystemDefault == true)
            {
                TempData["Error"] = _l["BacklogTools.CannotEdit"].Value;
                return RedirectToAction(nameof(Index));
            }
        }
        if (!ModelState.IsValid) return View(dto);
        await _service.UpdateAsync(dto);
        TempData["Success"] = _l["BacklogTools.Updated"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var tool = await _service.GetByIdAsync(id);
        if (tool == null) return NotFound();
        if (tool.IsSystemDefault)
        {
            TempData["Error"] = _l["BacklogTools.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["BacklogTools.Delete.Title"].Value;
        return View(tool);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var tool = await _service.GetByIdAsync(id);
        if (tool?.IsSystemDefault == true)
        {
            TempData["Error"] = _l["BacklogTools.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        await _service.DeleteAsync(id);
        TempData["Success"] = _l["BacklogTools.Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }
}
