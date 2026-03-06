using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using Prompteer.Infrastructure.Data;
using Prompteer.Web.Extensions;
using Prompteer.Web.Filters;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

try
{
    var connStr = Prompteer.Web.Extensions.ServiceCollectionExtensions.BuildConnectionString(builder.Configuration);
    var dbOpts  = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connStr).Options;
    using var tempDb = new AppDbContext(dbOpts);
    var entraKeys = new[]
    {
        "AzureAd:TenantId", 
        "AzureAd:ClientId", 
        "AzureAd:ClientSecret", 
        "AzureAd:Domain",
        "AzureAd:Instance",
        "AzureAd:CallbackPath"
    };
    var dbEntra = await tempDb.AppSettings
        .Where(s => entraKeys.Contains(s.Key))
        .ToDictionaryAsync(s => s.Key, s => (string?)s.Value);
    if (dbEntra.Count > 0)
        builder.Configuration.AddInMemoryCollection(dbEntra);
}
catch { }

var entraOverrides = new Dictionary<string, string?>();
void OverrideIfSet(string envVar, string configKey, string? defaultValue = null)
{
    var value = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrWhiteSpace(value))
        entraOverrides[configKey] = value;
    else if (defaultValue != null)
        entraOverrides[configKey] = defaultValue;
}
OverrideIfSet("ENTRA_TENANT_ID",     "AzureAd:TenantId");
OverrideIfSet("ENTRA_CLIENT_ID",     "AzureAd:ClientId");
OverrideIfSet("ENTRA_CLIENT_SECRET", "AzureAd:ClientSecret");
OverrideIfSet("ENTRA_DOMAIN",        "AzureAd:Domain");
OverrideIfSet("ENTRA_INSTANCE",      "AzureAd:Instance", "https://login.microsoftonline.com/");
OverrideIfSet("ENTRA_CALLBACK_PATH", "AzureAd:CallbackPath", "/signin-oidc");
if (entraOverrides.Count > 0)
    builder.Configuration.AddInMemoryCollection(entraOverrides);

var azureAdSection = builder.Configuration.GetSection("AzureAd");
if (!string.IsNullOrWhiteSpace(azureAdSection["TenantId"]) &&
    !string.IsNullOrWhiteSpace(azureAdSection["ClientId"]))
{
    builder.Services
        .AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd",
            openIdConnectScheme: "OpenIdConnect",
            cookieScheme: "EntraCookies",
            subscribeToOpenIdConnectMiddlewareDiagnosticsEvents: false)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

    builder.Services.AddAuthentication()
        .AddCookie("LocalAdmin", options =>
        {
            options.Cookie.Name      = ".AspNetCore.LocalAdmin";
            options.LoginPath        = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        })
        .AddPolicyScheme("Cookies", "Combined cookie auth", options =>
        {
            options.ForwardDefaultSelector = ctx =>
                ctx.Request.Cookies.ContainsKey(".AspNetCore.LocalAdmin")
                    ? "LocalAdmin"
                    : "EntraCookies";
        });

    builder.Services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
    {
        options.DefaultAuthenticateScheme = "Cookies";
        options.DefaultChallengeScheme    = "Cookies";
    });

    builder.Services.Configure<OpenIdConnectOptions>("OpenIdConnect", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "roles"
        };
    });

    builder.Services.Configure<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>(
        "EntraCookies", options =>
        {
            options.LoginPath        = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });
}
else
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "LocalAdmin";
        options.DefaultChallengeScheme    = "LocalAdmin";
    })
    .AddCookie("LocalAdmin", options =>
    {
        options.Cookie.Name      = ".AspNetCore.LocalAdmin";
        options.LoginPath        = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly",     p => p.RequireRole("Admin"));
    options.AddPolicy("EditorOrAbove", p => p.RequireRole("Admin", "Editor"));
});

var keysPath = Environment.GetEnvironmentVariable("DATAPROTECTION_KEYS_PATH")
    ?? Path.Combine(builder.Environment.ContentRootPath, "keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new System.IO.DirectoryInfo(keysPath));

builder.Services.AddMemoryCache();
builder.Services.AddLocalization();
builder.Services.AddScoped<AppSettingsViewDataFilter>();
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddMicrosoftIdentityUI()
    .AddMvcOptions(options => options.Filters.AddService<AppSettingsViewDataFilter>());
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("pt-BR") };

var defaultCulture = Environment.GetEnvironmentVariable("APP_LANGUAGE")
    ?? builder.Configuration["App:Language"]
    ?? "en";
if (!supportedCultures.Any(c => c.Name.Equals(defaultCulture, StringComparison.OrdinalIgnoreCase)))
    defaultCulture = "en";

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures     = supportedCultures,
    SupportedUICultures   = supportedCultures,
    RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider()
    ]
});

await app.Services.SeedDatabaseAsync();

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<Prompteer.Web.Middleware.SetupRedirectMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "MicrosoftIdentity",
    pattern: "MicrosoftIdentity/{controller=Account}/{action=SignIn}");

await app.RunAsync();

