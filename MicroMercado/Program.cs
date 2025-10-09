using Microsoft.EntityFrameworkCore;
using MicroMercado.Data;
using MicroMercado.Services; // Este incluye IClientService, IProductService
using MicroMercado.Services.sales; // <--- ¡¡¡AÑADIDO ESTE USING PARA TU SaleService!!!
using MicroMercado.DTOs; // Para ClientDTO
using MicroMercado.DTOs.Sales; // Para SaleDTO, etc.
using MicroMercado.Validators.Client;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Razor Pages
builder.Services.AddRazorPages();

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

// Configuración de FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Registro de Servicios
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleService, SaleService>(); // <-- Este registro ahora encontrará SaleService en MicroMercado.Services.sales

// Registro de Validadores de FluentValidation
builder.Services.AddScoped<IValidator<CreateClientDTO>, CreateClientValidator>();
builder.Services.AddScoped<IValidator<UpdateClientDTO>, UpdateClientValidator>();


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