using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Prompteer.Application.Services;

namespace Prompteer.Web.Filters;

public class AppSettingsViewDataFilter(IAppSettingService settings, IMemoryCache cache) : IAsyncActionFilter
{
    public const string CacheKey = "AppSetting:App:DateFormat";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is Controller controller)
        {
            if (!cache.TryGetValue(CacheKey, out string? dateFormat) || dateFormat is null)
            {
                dateFormat = await settings.GetAsync("App:DateFormat") ?? "MM/dd/yyyy";
                cache.Set(CacheKey, dateFormat, CacheDuration);
            }
            controller.ViewData["DateFormat"] = dateFormat;
            controller.ViewData["DateTimeFormat"] = dateFormat + " HH:mm";
        }
        await next();
    }
}
