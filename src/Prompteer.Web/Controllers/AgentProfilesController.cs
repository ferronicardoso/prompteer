using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Domain.Enums;

namespace Prompteer.Web.Controllers;

public class AgentProfilesController : Controller
{
    private readonly IAgentProfileService _service;
    private readonly IStringLocalizer<SharedResource> _l;

    public AgentProfilesController(IAgentProfileService service, IStringLocalizer<SharedResource> l)
    {
        _service = service;
        _l = l;
    }

    private void PopulateToneList(ToneType selected = ToneType.Technical)
    {
        ViewBag.ToneItems = new SelectList(new[]
        {
            new { V = (int)ToneType.Technical, T = _l["ToneType.Technical"].Value },
            new { V = (int)ToneType.Didactic, T = _l["ToneType.Didactic"].Value },
            new { V = (int)ToneType.Direct, T = _l["ToneType.Direct"].Value },
            new { V = (int)ToneType.Detailed, T = _l["ToneType.Detailed"].Value },
        }, "V", "T", (int)selected);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = _l["AgentProfiles.Title"].Value;
        ViewData["Search"] = search;
        var result = await _service.GetPagedAsync(page, 10, search);
        return View(result);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = _l["AgentProfiles.Create.Title"].Value;
        PopulateToneList();
        return View(new AgentProfileFormDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgentProfileFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            PopulateToneList(dto.Tone);
            return View(dto);
        }
        try
        {
            await _service.CreateAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(dto.Name), ex.Message);
            PopulateToneList(dto.Tone);
            return View(dto);
        }
        TempData["Success"] = _l["AgentProfiles.Created"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();
        ViewData["Title"] = profile.Name;
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(Guid id)
    {
        var clone = await _service.CloneAsync(id);
        TempData["Success"] = string.Format(_l["AgentProfiles.ClonedAs"].Value, clone.Name);
        return RedirectToAction(nameof(Edit), new { id = clone.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();
        if (profile.IsSystemDefault)
        {
            TempData["Error"] = _l["AgentProfiles.CannotEdit"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["AgentProfiles.Edit.Title"].Value;
        PopulateToneList(profile.Tone);
        var form = new AgentProfileFormDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Role = profile.Role,
            KnowledgeDomain = profile.KnowledgeDomain,
            Tone = profile.Tone,
            DefaultConstraints = profile.DefaultConstraints
        };
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AgentProfileFormDto dto)
    {
        if (dto.Id.HasValue)
        {
            var existing = await _service.GetByIdAsync(dto.Id.Value);
            if (existing?.IsSystemDefault == true)
            {
                TempData["Error"] = _l["AgentProfiles.CannotEdit"].Value;
                return RedirectToAction(nameof(Index));
            }
        }
        if (!ModelState.IsValid)
        {
            PopulateToneList(dto.Tone);
            return View(dto);
        }
        try
        {
            await _service.UpdateAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(dto.Name), ex.Message);
            PopulateToneList(dto.Tone);
            return View(dto);
        }
        TempData["Success"] = _l["AgentProfiles.Updated"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();
        if (profile.IsSystemDefault)
        {
            TempData["Error"] = _l["AgentProfiles.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = _l["AgentProfiles.Delete.Title"].Value;
        return View(profile);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile?.IsSystemDefault == true)
        {
            TempData["Error"] = _l["AgentProfiles.CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
        await _service.DeleteAsync(id);
        TempData["Success"] = _l["AgentProfiles.Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }
}
