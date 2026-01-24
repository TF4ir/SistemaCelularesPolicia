using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Recursos.Data;
using SistemaCelularesPolicia.Servicios.Interfaces;
using SistemaCelularesPolicia.Servicios.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// DbContext:
builder.Services.AddDbContext<SisCeluPoliC>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

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
