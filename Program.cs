using System.Globalization;
using GaragemVeiculos.Repositories;
using GaragemVeiculos.Services;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<ReservaService>();

var app = builder.Build();

var cultura = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new List<CultureInfo> { cultura },
    SupportedUICultures = new List<CultureInfo> { cultura }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reserva}/{action=Index}/{id?}");

app.Run();
