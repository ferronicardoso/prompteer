using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Prompteer.Web.Controllers;

[AllowAnonymous]
public class CultureController : Controller
{
    private static readonly HashSet<string> _supported =
        new(StringComparer.OrdinalIgnoreCase) { "en", "pt-BR" };

    // POST /Culture/SetCulture
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetCulture(string culture, string returnUrl = "/")
    {
        if (!_supported.Contains(culture))
            culture = "en";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires  = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }
}
