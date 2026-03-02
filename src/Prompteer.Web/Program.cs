using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using Prompteer.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─── Autenticação Microsoft Entra ID ─────────────────────────────────────────
var azureAdSection = builder.Configuration.GetSection("AzureAd");
if (!string.IsNullOrWhiteSpace(azureAdSection["TenantId"]) &&
    !string.IsNullOrWhiteSpace(azureAdSection["ClientId"]))
{
    builder.Services
        .AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd",
            openIdConnectScheme: "OpenIdConnect",
            cookieScheme: "Cookies",
            subscribeToOpenIdConnectMiddlewareDiagnosticsEvents: false)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

    // Force challenge to always show /Account/Login first.
    // The user explicitly clicks "Entrar com Microsoft" to trigger the OIDC flow.
    builder.Services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
    {
        options.DefaultChallengeScheme = "Cookies";
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
        "Cookies", options =>
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
        options.DefaultAuthenticateScheme = "Cookies";
        options.DefaultChallengeScheme    = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
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
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// ─── Seed do banco ────────────────────────────────────────────────────────────
await app.Services.SeedDatabaseAsync();

// ─── Pipeline HTTP ────────────────────────────────────────────────────────────
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

