using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.Services;
using Real_Estate_App.UnitOfWork;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Set "DatabaseProvider" to "SqlServer" or "Sqlite" in appsettings.Development.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionStringSql = builder.Configuration.GetConnectionString("DefaultConnectionSQL");
var provider = builder.Configuration["DatabaseProvider"];

if (provider == "SqlServer")
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionStringSql));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IPasswordHasher<User_Data>, PasswordHasher<User_Data>>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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
        var db = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<User_Data>>();

        if (provider != "SqlServer")
        {
            await db.Database.EnsureCreatedAsync();
        }

        if (await db.Database.CanConnectAsync())
        {
            if (!await db.UsersandAdminsset.AnyAsync(u => u.IsAdmin))
            {
                var existingByName = await db.UsersandAdminsset
                    .FirstOrDefaultAsync(u => u.UserName == "AdminUsername");

                if (existingByName != null)
                {
                    existingByName.IsAdmin = true;
                    existingByName.AdminRole = "Super";
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
                        AdminRole = "Super",
                    };
                    admin.Password = hasher.HashPassword(admin, "AdminPassword");
                    db.UsersandAdminsset.Add(admin);
                }
                await db.SaveChangesAsync();
            }

            // Backfill: legacy admin rows pre-date the AdminRole column and
            // would otherwise show as "no role" in the dashboard. Treat them
            // as Super to match how the login claim logic falls back today.
            var legacyAdmins = await db.UsersandAdminsset
                .Where(u => u.IsAdmin && (u.AdminRole == null || u.AdminRole == ""))
                .ToListAsync();
            if (legacyAdmins.Count > 0)
            {
                foreach (var legacy in legacyAdmins)
                {
                    legacy.AdminRole = "Super";
                }
                await db.SaveChangesAsync();
            }
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
    pattern: "{controller=Properties}/{action=Index}/{id?}");

app.Run();
