using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;
using Prompteer.Infrastructure.Data;
using Prompteer.Web.Helpers;
using Prompteer.Web.Models;

namespace Prompteer.Web.Controllers;

[AllowAnonymous]
public class SetupController : Controller
{
    private readonly AppDbContext _db;

    public SetupController(AppDbContext db) => _db = db;

    // GET /Setup
    public async Task<IActionResult> Index()
    {
        // Already set up — redirect to login
        if (await _db.ApplicationUsers.IgnoreQueryFilters().AnyAsync())
            return RedirectToAction("Login", "Account");

        return View(new SetupViewModel());
    }

    // POST /Setup
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SetupViewModel model)
    {
        if (await _db.ApplicationUsers.IgnoreQueryFilters().AnyAsync())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        // Create the bootstrap admin with local password
        var admin = new ApplicationUser
        {
            EntraObjectId = Guid.NewGuid().ToString(), // placeholder until Entra is linked
            DisplayName   = model.DisplayName.Trim(),
            Email         = model.Email.Trim().ToLowerInvariant(),
            Role          = UserRole.Admin,
            IsActive      = true,
            PasswordHash  = PasswordHasher.Hash(model.Password),
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };

        _db.ApplicationUsers.Add(admin);
        await _db.SaveChangesAsync();

        TempData["SetupSuccess"] = model.DisplayName;
        return RedirectToAction("Login", "Account");
    }
}
