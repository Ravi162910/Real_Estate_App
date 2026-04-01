using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Set "DatabaseProvider" to "SqlServer" or "Sqlite" in appsettings.Development.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

if (provider == "SqlServer")
{
    builder.Services.AddDbContext<UserAdminDbContext>(options => options.UseSqlServer(connectionString));
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<UserAdminDbContext>(options => options.UseSqlite(connectionString));
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();//allow cookie
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
