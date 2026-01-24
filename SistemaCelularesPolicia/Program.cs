using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Recursos.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Agregar aquí el DbContext:
builder.Services.AddDbContext<SisCeluPoliC>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


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
