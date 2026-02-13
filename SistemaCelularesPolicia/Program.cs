using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Recursos.Data;
using SistemaCelularesPolicia.Servicios.Interfaces;
using SistemaCelularesPolicia.Servicios.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

//var connectionString = "Server=tcp:server-pnp-sicir-2.database.windows.net,1433;Initial Catalog=bd-sicir;Persist Security Info=False;User ID=adminpnp;Password=Fabrizio#04;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
var connectionString = "Server=localhost\\SQL2025;Database=SistemaCelularesIncautados;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";

builder.Services.AddDbContext<SisCeluPoliC>(options =>
    options.UseSqlServer(connectionString));

// 2. CONFIGURAR LA AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Si un usuario no logueado intenta entrar a una zona privada, llévalo aquí:
        options.LoginPath = "/Account/Opciones";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // La sesión dura 20 mins de inactividad
        options.AccessDeniedPath = "/Home/AccessDenied"; // Si no tiene permisos
    });

builder.Services.AddScoped<IUsuarioPublico, UsuarioPublicoRepo>();

builder.Services.AddScoped<ICelular, CelularRepo>();

builder.Services.AddScoped<IPersonalPolicial, PersonalPolicialRepo>();

builder.Services.AddScoped<IEmail, EmailRepo>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
