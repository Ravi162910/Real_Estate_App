using System.Data.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Set "DatabaseProvider" to "SqlServer" or "Sqlite" in appsettings.Development.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionStringSql = builder.Configuration.GetConnectionString("DefaultConnectionSQL");
var provider = builder.Configuration["DatabaseProvider"];

if (provider == "SqlServer")
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionStringSql));
    builder.Services.AddDbContext<UsersPropertiesViewingDbContext>(options => options.UseSqlServer(connectionStringSql));
    //builder.Services.AddDbContext<>(options => options.UseSqlServer(connectionStringSql));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IPasswordHasher<User_Data>, PasswordHasher<User_Data>>();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/UserAdmin/Login";
        options.AccessDeniedPath = "/UserAdmin/Login";
    });//allow cookie
var app = builder.Build();

using (var scope = app.Services.CreateScope()) //Important for Seeding Database data & Auto Migrations when the app runs
{
    var services = scope.ServiceProvider;

    //PropertySeedData.Initilize(services);

    // Seed an admin user if none exists (or promote a legacy "AdminUsername" row)
    try
    {
        var db = services.GetRequiredService<UsersPropertiesViewingDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<User_Data>>();

        if (await db.Database.CanConnectAsync() && !await db.UsersandAdminsset.AnyAsync(u => u.IsAdmin))
        {
            var existingByName = await db.UsersandAdminsset
                .FirstOrDefaultAsync(u => u.UserName == "AdminUsername");

            if (existingByName != null)
            {
                existingByName.IsAdmin = true;
                existingByName.Password = hasher.HashPassword(existingByName, "AdminPassword");
            }
            else
            {
                var admin = new User_Data
                {
                    First_Name = "Admin",
                    Last_Name = "User",
                    Email = "admin@realestate.local",
                    UserName = "AdminUsername",
                    IsAdmin = true,
                };
                admin.Password = hasher.HashPassword(admin, "AdminPassword");
                db.UsersandAdminsset.Add(admin);
            }
            await db.SaveChangesAsync();
        }
    }
    catch (DbException)
    {
        // Schema not ready (migration pending). Ravi handles migrations after push.
    }
}


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
