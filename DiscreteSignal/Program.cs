using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiscreteSignal.Data;
using DiscreteSignal.Service.Implementation;
using DiscreteSignal.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Это у нас сохранение подстановок в интерфейс их реализации в ServiceCollection (опять же принцип D из SOLID)
// называется такая шняга DI (Dependency Injection)
// За Transient, Scoped, Singleton почитаешь уже сам (нам в проекте везде нужен Scoped)
builder.Services.AddScoped<IAudioStorageService, AudioStorageService>();
builder.Services.AddScoped<IDiscreteSpectrumService, DiscreteSpectrumService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();