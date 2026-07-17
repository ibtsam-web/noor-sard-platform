using Microsoft.EntityFrameworkCore;
using NoorSardPlatform.Data;
using NoorSardPlatform.Hubs;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuestPDF.Drawing;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.UseEnvironmentFonts = false;

var cairoRegularPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Fonts",
    "Cairo-Regular.ttf");

var cairoBoldPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Fonts",
    "Cairo-Bold.ttf");

FontManager.RegisterFont(File.OpenRead(cairoRegularPath));
FontManager.RegisterFont(File.OpenRead(cairoBoldPath));

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "لم يتم العثور على اتصال قاعدة البيانات."
    );

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "NoorSard.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
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
app.MapHub<DashboardHub>("/dashboardHub");
app.Run();
