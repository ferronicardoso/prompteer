using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Domain.Enums;

namespace Prompteer.Web.Controllers;

public class ArchitecturalPatternsController : Controller
{
    private readonly IArchitecturalPatternService _service;

    public ArchitecturalPatternsController(IArchitecturalPatternService service)
    {
        _service = service;
    }

    private void PopulateEcosystemList(TechEcosystem selected = TechEcosystem.Agnostic)
    {
        ViewBag.EcosystemItems = new SelectList(new[]
        {
            new { V = (int)TechEcosystem.DotNet,   T = ".NET" },
            new { V = (int)TechEcosystem.Node,     T = "Node.js" },
            new { V = (int)TechEcosystem.Python,   T = "Python" },
            new { V = (int)TechEcosystem.Java,     T = "Java" },
            new { V = (int)TechEcosystem.Agnostic, T = "Agnóstico" },
        }, "V", "T", (int)selected);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = "Padrões Arquiteturais";
        ViewData["Search"] = search;
        var result = await _service.GetPagedAsync(page, 10, search);
        return View(result);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Novo Padrão Arquitetural";
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
        TempData["Success"] = "Padrão arquitetural criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var pattern = await _service.GetByIdAsync(id);
        if (pattern == null) return NotFound();
        if (pattern.IsSystemDefault)
        {
            TempData["Error"] = "Padrões arquiteturais do sistema não podem ser editados.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = "Editar Padrão Arquitetural";
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
        TempData["Success"] = "Padrão arquitetural atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(Guid id)
    {
        var clone = await _service.CloneAsync(id);
        TempData["Success"] = $"Padrão clonado como \"{clone.Name}\".";
        return RedirectToAction(nameof(Edit), new { id = clone.Id });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var pattern = await _service.GetByIdAsync(id);
        if (pattern == null) return NotFound();
        if (pattern.IsSystemDefault)
        {
            TempData["Error"] = "Padrões arquiteturais padrão do sistema não podem ser excluídos.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = "Excluir Padrão Arquitetural";
        return View(pattern);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var pattern = await _service.GetByIdAsync(id);
        if (pattern?.IsSystemDefault == true)
        {
            TempData["Error"] = "Padrões arquiteturais padrão do sistema não podem ser excluídos.";
            return RedirectToAction(nameof(Index));
        }
        await _service.DeleteAsync(id);
        TempData["Success"] = "Padrão arquitetural excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
