using Microsoft.AspNetCore.Mvc;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;

namespace Prompteer.Web.Controllers;

public class BacklogToolsController : Controller
{
    private readonly IBacklogToolService _service;

    public BacklogToolsController(IBacklogToolService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewData["Title"] = "Ferramentas de Backlog";
        ViewData["Search"] = search;
        var result = await _service.GetPagedAsync(page, 10, search);
        return View(result);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Nova Ferramenta de Backlog";
        return View(new BacklogToolFormDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BacklogToolFormDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = "Ferramenta de backlog criada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tool = await _service.GetByIdAsync(id);
        if (tool == null) return NotFound();
        if (tool.IsSystemDefault)
        {
            TempData["Error"] = "Ferramentas padrão do sistema não podem ser editadas.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = "Editar Ferramenta de Backlog";
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
                TempData["Error"] = "Ferramentas padrão do sistema não podem ser editadas.";
                return RedirectToAction(nameof(Index));
            }
        }
        if (!ModelState.IsValid) return View(dto);
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Ferramenta de backlog atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var tool = await _service.GetByIdAsync(id);
        if (tool == null) return NotFound();
        if (tool.IsSystemDefault)
        {
            TempData["Error"] = "Ferramentas padrão do sistema não podem ser excluídas.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["Title"] = "Excluir Ferramenta de Backlog";
        return View(tool);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var tool = await _service.GetByIdAsync(id);
        if (tool?.IsSystemDefault == true)
        {
            TempData["Error"] = "Ferramentas padrão do sistema não podem ser excluídas.";
            return RedirectToAction(nameof(Index));
        }
        await _service.DeleteAsync(id);
        TempData["Success"] = "Ferramenta de backlog excluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
