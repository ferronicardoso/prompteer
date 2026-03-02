using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Domain.Enums;

namespace Prompteer.Web.Controllers;

public class AgentProfilesController : Controller
{
    private readonly IAgentProfileService _service;

    public AgentProfilesController(IAgentProfileService service)
    {
        _service = service;
    }

    private void PopulateToneList(ToneType selected = ToneType.Technical)
    {
        ViewBag.ToneItems = new SelectList(new[]
        {
            new { V = (int)ToneType.Technical, T = "Técnico" },
            new { V = (int)ToneType.Didactic, T = "Didático" },
            new { V = (int)ToneType.Direct, T = "Direto" },
            new { V = (int)ToneType.Detailed, T = "Detalhista" },
        }, "V", "T", (int)selected);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = "Perfis de Agente";
        ViewData["Search"] = search;
        var result = await _service.GetPagedAsync(page, 10, search);
        return View(result);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Novo Perfil de Agente";
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
        TempData["Success"] = "Perfil de agente criado com sucesso.";
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
        TempData["Success"] = $"Perfil clonado como \"{clone.Name}\".";
        return RedirectToAction(nameof(Edit), new { id = clone.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();
        if (profile.IsSystemDefault)
        {
            TempData["Error"] = "Perfis padrão do sistema não podem ser editados.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = "Editar Perfil de Agente";
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
                TempData["Error"] = "Perfis padrão do sistema não podem ser editados.";
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
        TempData["Success"] = "Perfil de agente atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null) return NotFound();
        if (profile.IsSystemDefault)
        {
            TempData["Error"] = "Perfis padrão do sistema não podem ser excluídos.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = "Excluir Perfil de Agente";
        return View(profile);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile?.IsSystemDefault == true)
        {
            TempData["Error"] = "Perfis padrão do sistema não podem ser excluídos.";
            return RedirectToAction(nameof(Index));
        }
        await _service.DeleteAsync(id);
        TempData["Success"] = "Perfil de agente excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
