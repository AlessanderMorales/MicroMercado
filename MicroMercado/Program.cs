using Microsoft.EntityFrameworkCore;
using MicroMercado.Data;
using MicroMercado.Services;
using MicroMercado.Services.sales;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

    // Add services - RAZOR PAGES CON JSON OPTIONS
    builder.Services.AddRazorPages(options =>
    {
    options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // ← IMPORTANTE
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });



builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleService, SaleService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();