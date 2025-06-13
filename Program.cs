using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 
using PopularBookstore.Services;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Ensure a connection string is found
if (string.IsNullOrEmpty(connectionString))
{
   
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json. Please ensure it is configured correctly.");
}

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index");
});

builder.Services.AddTransient<IEmailSender, MessageSender>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Path to login page
        options.AccessDeniedPath = "/AccessDenied"; // Path to access denied page
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie expiration
        options.SlidingExpiration = true; // Resets expiration on activity
    });

var app = builder.Build();

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

public partial class Program { } 