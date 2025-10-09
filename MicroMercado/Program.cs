using Microsoft.EntityFrameworkCore;
using MicroMercado.Data;
using MicroMercado.Services;
using MicroMercado.DTOs; 
using MicroMercado.Validators.Client; 
using FluentValidation; 
using FluentValidation.AspNetCore; 
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});


builder.Services.AddFluentValidationAutoValidation(); 
builder.Services.AddFluentValidationClientsideAdapters();


builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IValidator<CreateClientDTO>, CreateClientValidator>();
builder.Services.AddScoped<IValidator<UpdateClientDTO>, UpdateClientValidator>();



builder.Services.AddScoped<IProductService, ProductService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
