using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text.Json;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using MicroMercado.Application.Validators.Client;
using MicroMercado.Application.Validators.Product;
using MicroMercado.Infrastructure.Data;
using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.Validators.Category;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Presentation/Pages";
})
    .AddMvcOptions(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});


// Configuración de la base de datos PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Configuración de FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Registro de Servicios
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();


// Registro de Validadores de FluentValidation
builder.Services.AddScoped<IValidator<CreateClientDTO>, CreateClientValidator>();
builder.Services.AddScoped<IValidator<UpdateClientDTO>, UpdateClientValidator>();

builder.Services.AddScoped<IValidator<CreateProductDTO>, CreateProductValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDTO>, UpdateProductValidator>();

builder.Services.AddScoped<IValidator<CreateCategoryDTO>, CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDTO>, UpdateCategoryValidator>();


// Configuración de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuración de los controladores y la serialización JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

// Configuración del pipeline HTTP
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