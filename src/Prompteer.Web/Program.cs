using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using Prompteer.Web.Extensions;
using Prompteer.Web.Filters;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ─── Entra ID via environment variables (Docker Swarm / containers) ──────────
// Supports ENTRA_TENANT_ID, ENTRA_CLIENT_ID, ENTRA_CLIENT_SECRET, ENTRA_DOMAIN.
// Uses AddInMemoryCollection so the values are stored in a dedicated memory
// source and are NOT lost if the JSON file provider reloads at runtime.
var entraOverrides = new Dictionary<string, string?>();
void OverrideIfSet(string envVar, string configKey)
{
    var value = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrWhiteSpace(value))
        entraOverrides[configKey] = value;
}
OverrideIfSet("ENTRA_TENANT_ID",     "AzureAd:TenantId");
OverrideIfSet("ENTRA_CLIENT_ID",     "AzureAd:ClientId");
OverrideIfSet("ENTRA_CLIENT_SECRET", "AzureAd:ClientSecret");
OverrideIfSet("ENTRA_DOMAIN",        "AzureAd:Domain");
if (entraOverrides.Count > 0)
    builder.Configuration.AddInMemoryCollection(entraOverrides);

// ─── Autenticação Microsoft Entra ID ─────────────────────────────────────────
var azureAdSection = builder.Configuration.GetSection("AzureAd");
if (!string.IsNullOrWhiteSpace(azureAdSection["TenantId"]) &&
    !string.IsNullOrWhiteSpace(azureAdSection["ClientId"]))
{
    // MSAL gerencia apenas o scheme "EntraCookies" para usuários Entra.
    // O admin local usa o scheme "LocalAdmin", completamente fora do alcance do MSAL.
    // Assim, o OnValidatePrincipal do MSAL nunca interfere na sessão do admin local.
    builder.Services
        .AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd",
            openIdConnectScheme: "OpenIdConnect",
            cookieScheme: "EntraCookies",
            subscribeToOpenIdConnectMiddlewareDiagnosticsEvents: false)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

    // "LocalAdmin" — cookie simples, sem nenhum envolvimento do MSAL.
    // "Cookies"    — policy scheme (DefaultAuthenticateScheme / DefaultChallengeScheme).
    //                Despacha para "LocalAdmin" quando esse cookie está presente,
    //                caso contrário para "EntraCookies".
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

    // DefaultAuthenticateScheme e DefaultChallengeScheme apontam para o policy scheme.
    builder.Services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
    {
        options.DefaultAuthenticateScheme = "Cookies";
        options.DefaultChallengeScheme    = "Cookies";
    });

    // Map the "roles" App Roles claim so User.IsInRole("Admin") works
    builder.Services.Configure<OpenIdConnectOptions>("OpenIdConnect", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // "roles" claim from Entra App Roles → ASP.NET Core role checks
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
    // Sem configuração Entra — cookie simples para desenvolvimento local
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

// ─── Autorização ──────────────────────────────────────────────────────────────
// Roles são definidos no App Registration do Entra (App Roles: Admin, Editor, Viewer)
// e atribuídos em Enterprise Applications → Users and Groups.
builder.Services.AddAuthorization(options =>
{
    // Require authentication globally — controllers/actions opt out with [AllowAnonymous]
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly",     p => p.RequireRole("Admin"));
    options.AddPolicy("EditorOrAbove", p => p.RequireRole("Admin", "Editor"));
});

// ─── Serviços ────────────────────────────────────────────────────────────────
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

// ─── Localização ──────────────────────────────────────────────────────────────
// Supported cultures: English (default) and Brazilian Portuguese.
// Priority: 1. Cookie  2. APP_LANGUAGE env var  3. app setting  4. default (en)
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("pt-BR") };

// Resolve default culture: env APP_LANGUAGE → config App:Language → "en"
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

// ─── Seed do banco ────────────────────────────────────────────────────────────
await app.Services.SeedDatabaseAsync();

// ─── Pipeline HTTP ────────────────────────────────────────────────────────────

// Respeita X-Forwarded-Proto/For quando rodando atrás de reverse proxy (Docker/nginx).
// Isso garante que as redirect URIs geradas usem https:// corretamente.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

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

// Rotas do Microsoft Identity Web UI (signin-oidc, signout-callback-oidc)
app.MapControllerRoute(
    name: "MicrosoftIdentity",
    pattern: "MicrosoftIdentity/{controller=Account}/{action=SignIn}");

app.Run();

