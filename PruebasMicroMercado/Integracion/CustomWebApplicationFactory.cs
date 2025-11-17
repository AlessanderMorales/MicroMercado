using MicroMercado.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace PruebasMicroMercado.Integracion
{

    /// Factory personalizado para pruebas de integración con base de datos InMemory.

    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid());
                    options.EnableSensitiveDataLogging();
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

                    db.Database.EnsureCreated();

                    try
                    {
                        SeedTestData(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error al inicializar la base de datos de pruebas.");
                    }
                }
            });

            builder.UseEnvironment("Testing");
        }


        /// Genera datos de prueba en la base de datos.
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
