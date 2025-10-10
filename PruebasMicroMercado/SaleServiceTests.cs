using MicroMercado.Data;
using MicroMercado.DTOs.Sales;
using MicroMercado.Models;
using MicroMercado.Services.sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado
{
    public class SaleServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<ILogger<SaleService>> GetMockLogger()
        {
            return new Mock<ILogger<SaleService>>();
        }

        private async Task SeedTestData(ApplicationDbContext context)
        {
            var category = new Category
            {
                Id = 1,
                Name = "Electrónicos",
                Status = 1,
                LastUpdate = DateTime.Now
            };
            context.Categories.Add(category);

            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
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
                    Name = "Mouse",
                    Description = "Mouse inalámbrico",
                    Brand = "Logitech",
                    Price = 25.00m,
                    Stock = 5,
                    CategoryId = 1,
                    Status = 1,
                    LastUpdate = DateTime.Now
                },
                new Product
                {
                    Id = 3,
                    Name = "Teclado",
                    Description = "Teclado mecánico",
                    Brand = "Corsair",
                    Price = 100.00m,
                    Stock = 0,
                    CategoryId = 1,
                    Status = 1,
                    LastUpdate = DateTime.Now
                }
            };
            context.Products.AddRange(products);

            var client = new Client
            {
                Id = 1,
                Name = "Juan",
                LastName = "Pérez",
                TaxDocument = "12345678",
                Status = 1,
                LastUpdate = DateTime.Now
            };
            context.Clients.Add(client);

            await context.SaveChangesAsync();
        }

        // Test 1: CreateSaleAsync - Complexity 3 - Path 1 (Successful sale)
        [Fact]
        public async Task CreateSaleAsync_ShouldCreateSale_WhenDataIsValid()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var saleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = 1,
                TotalAmount = 1525.00m,
                CashReceived = 1600.00m,
                Change = 75.00m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 1500.00m },
                    new SaleDTO.SaleItemDTO { ProductId = 2, Quantity = 1, Price = 25.00m }
                }
            };
            var result = await service.CreateSaleAsync(saleDTO);
            Assert.True(result.Success);
            Assert.Equal("Venta registrada exitosamente", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.ItemsCount);
        }

        // Test 2: CreateSaleAsync - Complexity 3 - Path 2 (Validation fails)
        [Fact]
        public async Task CreateSaleAsync_ShouldReturnError_WhenValidationFails()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var saleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = 1,
                TotalAmount = 100.00m,
                CashReceived = 100.00m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>()
            };
            var result = await service.CreateSaleAsync(saleDTO);
            Assert.False(result.Success);
            Assert.Equal("No hay productos en la venta", result.Message);
        }

        // Test 3: CreateSaleAsync - Complexity 3 - Path 3 (Exception occurs)
        [Fact]
        public async Task CreateSaleAsync_ShouldReturnError_WhenExceptionOccurs()
        {
            var context = GetInMemoryDbContext();
            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var saleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = 999,
                TotalAmount = 100.00m,
                CashReceived = 100.00m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO { ProductId = 999, Quantity = 1, Price = 100.00m }
                }
            };
            var result = await service.CreateSaleAsync(saleDTO);
            Assert.False(result.Success);
            Assert.Equal("Error al procesar la venta", result.Message);
        }

        // Test 4: ValidateStockAsync - Complexity 3 - Path 1 (All stock valid)
        [Fact]
        public async Task ValidateStockAsync_ShouldReturnSuccess_WhenStockIsSufficient()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var items = new List<SaleDTO.SaleItemDTO>
            {
                new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 2, Price = 1500.00m },
                new SaleDTO.SaleItemDTO { ProductId = 2, Quantity = 1, Price = 25.00m }
            };
            var result = await service.ValidateStockAsync(items);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        // Test 5: ValidateStockAsync - Complexity 3 - Path 2 (Insufficient stock)
        [Fact]
        public async Task ValidateStockAsync_ShouldReturnError_WhenStockIsInsufficient()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var items = new List<SaleDTO.SaleItemDTO>
            {
                new SaleDTO.SaleItemDTO { ProductId = 3, Quantity = 1, Price = 100.00m } // Product with 0 stock
            };
            var result = await service.ValidateStockAsync(items);
            Assert.False(result.Success);
            Assert.Equal("Validación de stock fallida", result.Message);
            Assert.NotEmpty(result.Errors);
        }

        // Test 6: ValidateStockAsync - Complexity 3 - Path 3 (Exception occurs)
        [Fact]
        public async Task ValidateStockAsync_ShouldReturnError_WhenExceptionOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var items = new List<SaleDTO.SaleItemDTO>
            {
                new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 100.00m }
            };
            var result = await service.ValidateStockAsync(items);
            Assert.False(result.Success);
            Assert.Equal("Error al validar stock", result.Message);
        }

        // Test 7: UpdateProductStockAsync - Complexity 2 - Path 1 (Successful update)
        [Fact]
        public async Task UpdateProductStockAsync_ShouldReturnTrue_WhenUpdateSucceeds()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var items = new List<SaleDTO.SaleItemDTO>
            {
                new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 2, Price = 1500.00m }
            };
            var result = await service.UpdateProductStockAsync(items);
            Assert.True(result);
            var product = await context.Products.FindAsync((short)1);
            Assert.Equal(8, product.Stock);
        }

        // Test 8: UpdateProductStockAsync - Complexity 2 - Path 2 (Exception occurs)
        [Fact]
        public async Task UpdateProductStockAsync_ShouldReturnFalse_WhenExceptionOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);

            // Dispose context to force an exception
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var service = new SaleService(context, logger.Object);

            var items = new List<SaleDTO.SaleItemDTO>
            {
                new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 100.00m }
            };
            var result = await service.UpdateProductStockAsync(items);
            Assert.False(result);
        }
    }
}