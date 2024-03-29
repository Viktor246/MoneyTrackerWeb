using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using Microsoft.AspNetCore.Identity;
using MoneyTracker.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using MoneyTracker.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddDbContext<ApplicationUserContext>(options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationUserContext>();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("isOwner", policy =>
        policy.Requirements.Add(new IsOwnerRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, CategoryAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SubCategoryAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ExpenseAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, BalanceAuthorizationHandler>();


builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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
app.UseAuthentication();;

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
