using MicroMercado.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace PruebasMicroMercado.Integracion
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly string _databaseName;

        public CustomWebApplicationFactory()
        {
            // Cada instancia de factory tiene su propia base de datos única
            _databaseName = $"IntegrationTestDb_{Guid.NewGuid()}";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remover el DbContext configurado en Program.cs
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Agregar DbContext con base de datos única para esta prueba
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    // Ignorar advertencias de transacciones no soportadas en InMemory
                    options.ConfigureWarnings(w => 
                        w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            });

            builder.UseEnvironment("Testing");
        }

        public void SeedDatabase()
        {
            using var scope = Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            var logger = services.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

            try
            {
                context.Database.EnsureCreated();
                SeedTestData(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al inicializar la base de datos de pruebas.");
                throw;
            }
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            context.Categories.RemoveRange(context.Categories);
            context.Products.RemoveRange(context.Products);
            context.Clients.RemoveRange(context.Clients);
            context.SaveChanges();

            var categories = new[]
            {
                new MicroMercado.Domain.Models.Category
                {
                    Id = 1,
                    Name = "Lácteos",
                    Description = "Productos lácteos varios",
                    Status = 1,
                    LastUpdate = DateTime.Now
                },
                new MicroMercado.Domain.Models.Category
                {
                    Id = 2,
                    Name = "Alimentos",
                    Description = "Alimentos diversos",
                    Status = 1,
                    LastUpdate = DateTime.Now
                }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = new[]
            {
                new MicroMercado.Domain.Models.Product
                {
                    Id = 1,
                    Name = "Yogurt Natural",
                    Description = "Yogurt natural 1L",
                    Brand = "Pil",
                    Price = 10.00m,
                    Stock = 50,
                    CategoryId = 1,
                    Status = 1,
                    LastUpdate = DateTime.Now
                },
                new MicroMercado.Domain.Models.Product
                {
                    Id = 2,
                    Name = "Leche Entera",
                    Description = "Leche entera 1L",
                    Brand = "Pil",
                    Price = 8.50m,
                    Stock = 100,
                    CategoryId = 1,
                    Status = 1,
                    LastUpdate = DateTime.Now
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();

            var clients = new[]
            {
                new MicroMercado.Domain.Models.Client
                {
                    Id = 1,
                    BusinessName = "Cliente Test",
                    Email = "test@email.com",
                    TaxDocument = "12345678",
                    Address = "Av. Test 123",
                    Status = 1,
                    LastUpdate = DateTime.Now
                }
            };

            context.Clients.AddRange(clients);
            context.SaveChanges();
        }
    }
}
