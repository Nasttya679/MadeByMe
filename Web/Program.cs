using MadeByMe.Application.Common;
using MadeByMe.Application.Services;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories;
using MadeByMe.Infrastructure.Repositories.Implementations;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ProjectSettings>(
    builder.Configuration.GetSection("ProjectSettings"));

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

string? connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    });
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IBuyerCartRepository, BuyerCartRepository>();
builder.Services.AddScoped<IBuyerCartService, BuyerCartService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

app.UseMiddleware<MadeByMe.Web.Middlewares.ExceptionHandlingMiddleware>();
app.UseMiddleware<MadeByMe.Web.Middlewares.ExecutionTimeMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var wwwrootPath = app.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
var imagesPath = Path.Combine(wwwrootPath, "images");

if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<MadeByMe.Web.Middlewares.RequestLoggingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "User", "Seller", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Помилка при ініціалізації БД");
    }
}

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();