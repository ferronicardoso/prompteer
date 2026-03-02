using Prompteer.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─── Serviços ────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

