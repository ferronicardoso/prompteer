using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Domain.Enums;

namespace Prompteer.Web.Controllers;

public class TechnologiesController : Controller
{
    private readonly ITechnologyService _service;

    public TechnologiesController(ITechnologyService service)
    {
        _service = service;
    }

    private void PopulateCategoryList(TechCategory? selected = null)
    {
        ViewBag.CategoryItems = new SelectList(new[]
        {
            new { V = (int)TechCategory.Framework,     T = "Framework" },
            new { V = (int)TechCategory.Database,      T = "Banco de Dados" },
            new { V = (int)TechCategory.ORM,           T = "ORM" },
            new { V = (int)TechCategory.Frontend,      T = "Frontend" },
            new { V = (int)TechCategory.Auth,          T = "Autenticação" },
            new { V = (int)TechCategory.Messaging,     T = "Mensageria" },
            new { V = (int)TechCategory.Cache,         T = "Cache" },
            new { V = (int)TechCategory.Observability, T = "Observabilidade" },
            new { V = (int)TechCategory.DevOps,        T = "DevOps" },
            new { V = (int)TechCategory.Testing,       T = "Testes" },
            new { V = (int)TechCategory.Other,         T = "Outro" },
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
            new { V = (int)TechEcosystem.Agnostic, T = "Agnóstico" },
        }, "V", "T", selected.HasValue ? (int?)selected.Value : null);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, TechCategory? category = null, TechEcosystem? ecosystem = null)
    {
        ViewData["Title"] = "Tecnologias";
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
        ViewData["Title"] = "Nova Tecnologia";
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
        TempData["Success"] = "Tecnologia criada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tech = await _service.GetByIdAsync(id);
        if (tech == null) return NotFound();
        ViewData["Title"] = "Editar Tecnologia";
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
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Tecnologia atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var tech = await _service.GetByIdAsync(id);
        if (tech == null) return NotFound();
        ViewData["Title"] = "Excluir Tecnologia";
        return View(tech);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Tecnologia excluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
