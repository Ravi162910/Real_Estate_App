using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.Services;
using Real_Estate_App.UnitOfWork;
using System.Data.Common;
using System.Threading.RateLimiting;

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
builder.Services.AddMemoryCache();

// Stripe: bind the "Stripe" config section and register the payment
// services. The secret key is applied process-wide for Stripe.net.
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<ICheckoutFulfillmentService, CheckoutFulfillmentService>();
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    Stripe.StripeConfiguration.ApiKey = stripeSecretKey;
}
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/UserAdmin/Login";
        options.AccessDeniedPath = "/UserAdmin/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });//allow cookie

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("checkout", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Public contact form sends email - cap submissions per IP to limit spam.
    options.AddPolicy("contact", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

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

            // Demo accounts: one admin per sub-role so each role's capabilities
            // can be shown without manually promoting a user in the dashboard.
            // Development only - these have known passwords, so we never seed
            // them on a deployed (e.g. Azure) environment. Idempotent: each row
            // is only inserted when its username is missing.
            if (app.Environment.IsDevelopment())
            {
                await EnsureSeedUserAsync("PropertyAdmin", "Property", "Admin",
                    "property.admin@realestate.local", "PropertyPassword",
                    isAdmin: true, adminRole: "Property", isAgent: false);

                await EnsureSeedUserAsync("TransactionAdmin", "Transaction", "Admin",
                    "transaction.admin@realestate.local", "TransactionPassword",
                    isAdmin: true, adminRole: "Transaction", isAgent: false);
            }
        }

        if (!await db.UsersandAdminsset.AnyAsync(u => u.IsAgent)) 
        {
            var anyagents = await db.UsersandAdminsset.FirstOrDefaultAsync(u => u.UserName == "AgentUsername");

            if (anyagents != null)
            {
                anyagents.Password = hasher.HashPassword(anyagents, anyagents.Password);
                anyagents.IsAgent = true;
            }
            else 
            {
                var seededagent = new User_Data
                {
                    First_Name = "Agent",
                    Last_Name = "User",
                    Email = "agent@gmail.com",
                    UserName = "AgentUsername",
                    IsAdmin = false,
                    IsAgent = true
                };

                seededagent.Password = hasher.HashPassword(anyagents, "AgentPassword");
                db.UsersandAdminsset.Add(seededagent);

            }
            await db.SaveChangesAsync();
        }

        // Inserts a user row only when the username does not already exist.
        // Keeps the demo-account seeding above idempotent across restarts.
        async Task EnsureSeedUserAsync(string userName, string firstName, string lastName,
            string email, string password, bool isAdmin, string? adminRole, bool isAgent)
        {
            if (await db.UsersandAdminsset.AnyAsync(u => u.UserName == userName))
            {
                return;
            }

            var user = new User_Data
            {
                First_Name = firstName,
                Last_Name = lastName,
                Email = email,
                UserName = userName,
                IsAdmin = isAdmin,
                AdminRole = adminRole,
                IsAgent = isAgent,
            };
            user.Password = hasher.HashPassword(user, password);
            db.UsersandAdminsset.Add(user);
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

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // 'unsafe-inline' is required for now: ~13 inline style="..." attributes and
    // 5 inline onclick handlers across views. Tightening to nonces/hashes is an
    // A3 refactor. Stripe origins are pre-allowlisted for the payment flow.
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://js.stripe.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
        "connect-src 'self' https://api.stripe.com; " +
        "frame-src https://js.stripe.com https://hooks.stripe.com https://checkout.stripe.com; " +
        "form-action 'self' https://checkout.stripe.com; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "object-src 'none'";

    await next();
});

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Attribute-routed controllers (e.g. the Stripe webhook at /stripe/webhook).
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Properties}/{action=Index}/{id?}");

app.Run();
