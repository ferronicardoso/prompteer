using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prompteer.Domain.Enums;
using Prompteer.Infrastructure.Data;

namespace Prompteer.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    // GET /Users
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 20;

        var query = _db.ApplicationUsers
            .IgnoreQueryFilters()
            .OrderBy(u => u.DisplayName)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.DisplayName.Contains(search) ||
                u.Email.Contains(search));

        var total = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["Title"] = "Gerenciamento de Usuários";
        ViewBag.Search   = search;
        ViewBag.Page     = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total    = total;
        return View(users);
    }

    // GET /Users/Edit/id — only IsActive is editable; role is managed in Entra
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _db.ApplicationUsers.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        ViewData["Title"] = "Editar Usuário";
        return View(user);
    }

    // POST /Users/Edit/id
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, bool isActive)
    {
        var user = await _db.ApplicationUsers.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        user.IsActive  = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Usuário \"{user.DisplayName}\" atualizado.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Users/Deactivate/id
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var user = await _db.ApplicationUsers.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        user.IsActive  = false;
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Usuário \"{user.DisplayName}\" desativado.";
        return RedirectToAction(nameof(Index));
    }
}
