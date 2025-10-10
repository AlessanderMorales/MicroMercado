using MicroMercado.Application.Services;
using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado
{
    public class ProductServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<ILogger<ProductService>> GetMockLogger()
        {
            return new Mock<ILogger<ProductService>>();
        }

        private async Task SeedTestData(ApplicationDbContext context)
        {
            var category1 = new Category
            {
                Id = 1,
                Name = "Electrónicos",
                Status = 1,
                LastUpdate = DateTime.Now
            };

            var category2 = new Category
            {
                Id = 2,
                Name = "Alimentos",
                Status = 1,
                LastUpdate = DateTime.Now
            };

            context.Categories.AddRange(category1, category2);

            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop Dell",
                    Description = "Laptop de alta gama",
                    Brand = "Dell",
                    Price = 1500.00m,
                    Stock = 10,
                    CategoryId = 1,
                    Status = 1,
                    LastUpdate = DateTime.Now
                },
                new Product
                {
                    Id = 2,
                    Name = "Mouse Logitech",
                    Description = "Mouse inalámbrico",
                    Brand = "Logitech",
                    Price = 25.00m,
                    Stock = 50,
                    CategoryId = 1,
                    Status = 1,
                    LastUpdate = DateTime.Now
                },
                new Product
                {
                    Id = 3,
                    Name = "Arroz Integral",
                    Description = "Arroz de grano largo",
                    Brand = "Campo Verde",
                    Price = 5.00m,
                    Stock = 0,
                    CategoryId = 2,
                    Status = 1,
                    LastUpdate = DateTime.Now
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        // Test 1: SearchProductsAsync - Complexity 3 - Path 1 (Empty search term)
        [Fact]
        public async Task SearchProductsAsync_ShouldReturnEmpty_WhenSearchTermIsEmpty()
        {
            var context = GetInMemoryDbContext();
            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            var result = await service.SearchProductsAsync("");
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // Test 2: SearchProductsAsync - Complexity 3 - Path 2 (Valid search term)
        [Fact]
        public async Task SearchProductsAsync_ShouldReturnProducts_WhenSearchTermIsValid()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            var result = await service.SearchProductsAsync("laptop");
            Assert.NotNull(result);
            var products = result.ToList();
            Assert.Single(products);
            Assert.Equal("Laptop Dell", products[0].Name);
            Assert.Equal("Electrónicos", products[0].CategoryName);
        }

        // Test 3: SearchProductsAsync - Complexity 3 - Path 3 (Exception occurs)
        [Fact]
        public async Task SearchProductsAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.SearchProductsAsync("laptop");
            });
        }

        // Test 4: GetProductByIdAsync - Complexity 2 - Path 1 (Product exists)
        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            var result = await service.GetProductByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Laptop Dell", result.Name);
            Assert.Equal(1500.00m, result.Price);
            Assert.Equal(10, result.Stock);
            Assert.Equal("Electrónicos", result.CategoryName);
        }

        // Test 5: GetProductByIdAsync - Complexity 2 - Path 2 (Exception occurs)
        [Fact]
        public async Task GetProductByIdAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.GetProductByIdAsync(1);
            });
        }

        // Test 6: HasStockAsync - Complexity 3 - Path 1 (Has sufficient stock)
        [Fact]
        public async Task HasStockAsync_ShouldReturnTrue_WhenStockIsSufficient()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            var result = await service.HasStockAsync(1, 5);
            Assert.True(result);
        }

        // Test 7: HasStockAsync - Complexity 3 - Path 2 (Insufficient stock)
        [Fact]
        public async Task HasStockAsync_ShouldReturnFalse_WhenStockIsInsufficient()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            var result = await service.HasStockAsync(1, 20);
            Assert.False(result);
        }

        // Test 8: HasStockAsync - Compleity 3 - Path 3(Exception occurs) 
        [Fact]                 
        public async Task HasStockAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var service = new ProductService(context, logger.Object);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.HasStockAsync(1, 5);
            });
        }
    }
}