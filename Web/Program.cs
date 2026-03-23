using MadeByMe.Domain.Entities;
using MadeByMe.Application.Services;
using MadeByMe.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Infrastructure.Repositories.Implementations;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// 1. Підключення до БД
// ---------------------------
string connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string not found");

// ---------------------------
// 2. DbContext з ретраями
// ---------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// ---------------------------
// 3. Controllers + Views
// ---------------------------
builder.Services.AddControllersWithViews();

// ---------------------------
// 4. Сервіси та Identity
// ---------------------------
builder.Services.AddHttpContextAccessor();

// AddScoped для всіх сервісів і репозиторіїв
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IBuyerCartRepository, BuyerCartRepository>();
builder.Services.AddScoped<IBuyerCartService, BuyerCartService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

// ---------------------------
// 5. Static files
// ---------------------------
builder.Services.AddDirectoryBrowser();
var app = builder.Build();

// Ensure wwwroot/images
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(wwwrootPath);
Directory.CreateDirectory(Path.Combine(wwwrootPath, "images"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(wwwrootPath),
    RequestPath = "",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

// ---------------------------
// 6. Seed ролей
// ---------------------------
async Task SeedRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "User", "Seller", "Admin" };
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
}

// ---------------------------
// 7. Міграції
// ---------------------------
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database initialization failed");
    throw;
}

// ---------------------------
// 8. Middleware
// ---------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Seed ролей
using (var scope = app.Services.CreateScope())
{
    await SeedRoles(scope.ServiceProvider);
}

// ---------------------------
// 9. Запуск на HTTP
// ---------------------------
string urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5000";
app.Urls.Clear();
app.Urls.Add(urls);

app.Run();