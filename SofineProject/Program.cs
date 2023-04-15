using Microsoft.EntityFrameworkCore;
using SofineProject.DataAccessLayer;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddHttpContextAccessor();
var app = builder.Build();
app.UseStaticFiles();
app.MapControllerRoute("default", "{controller=home}/{action=Index}/{id?}");
app.Run();
