using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PopularBookstore.Services;
using WebApplication1.Data;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity services with support for Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Relax password requirements for development
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
    .AddRoles<IdentityRole>() // This is crucial for role management
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index");
    options.Conventions.AuthorizePage("/Books/Index", "AdminOnly");

    // Allow anonymous access to login-related pages
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/CreateAccount");
    options.Conventions.AllowAnonymousToPage("/AdminLogin");
    options.Conventions.AllowAnonymousToPage("/AdminPortal");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddTransient<IEmailSender, MessageSender>();
builder.Services.AddScoped<CartService>();

var app = builder.Build();

// Seed the database with the admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        await SeedAdminUser(userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the admin user.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

async Task SeedAdminUser(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
{
    const string adminRole = "Admin";
    const string adminEmail = "admin@example.com";
    const string adminPassword = "admin123!";

    logger.LogInformation("Starting admin user seeding process...");

    try
    {
        // Check and create Admin role
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            logger.LogInformation("Creating Admin role...");
            var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
            if (roleResult.Succeeded)
            {
                logger.LogInformation("Admin role created successfully.");
            }
            else
            {
                logger.LogError("Failed to create Admin role: {Errors}",
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return;
            }
        }
        else
        {
            logger.LogInformation("Admin role already exists.");
        }

        // Check if admin user exists
        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        if (existingUser == null)
        {
            logger.LogInformation("Creating admin user: {Email}", adminEmail);
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, adminPassword);
            if (createResult.Succeeded)
            {
                logger.LogInformation("Admin user created successfully.");

                var roleResult = await userManager.AddToRoleAsync(user, adminRole);
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin role assigned to user successfully.");
                }
                else
                {
                    logger.LogError("Failed to assign Admin role to user: {Errors}",
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);

            var hasAdminRole = await userManager.IsInRoleAsync(existingUser, adminRole);
            if (!hasAdminRole)
            {
                logger.LogInformation("Adding Admin role to existing user...");
                var roleResult = await userManager.AddToRoleAsync(existingUser, adminRole);
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin role assigned to existing user successfully.");
                }
                else
                {
                    logger.LogError("Failed to assign Admin role to existing user: {Errors}",
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("User already has Admin role.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Exception occurred during admin user seeding.");
    }

    logger.LogInformation("Admin user seeding process completed.");
}