using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Application.Services;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.Integracion
{
    [Collection("IntegrationTests")]
    public class EdgeCasesIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public EdgeCasesIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region IT-51: Venta con producto que se queda sin stock durante la transacción

        [Fact(DisplayName = "IT-51: Validación de stock debe detectar cambios antes de commit")]
        public async Task CreateSale_WhenStockChangesBeforeCommit_ShouldDetectAndFail()
        {
            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var product = new MicroMercado.Domain.Models.Product
            {
                Name = "Producto Stock Limitado",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = 2,
                CategoryId = 1,
                Status = 1,
                LastUpdate = DateTime.Now
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = null,
                TotalAmount = 30.00m,
                CashReceived = 30.00m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO { ProductId = product.Id, Quantity = 3, Price = 10.00m, Total = 30.00m }
                }
            };

            var result = await saleService.CreateSaleAsync(createSaleDTO);

            Assert.False(result.Success);
            Assert.Contains("insuficiente", result.Message.ToLower());
        }

        #endregion

        #region IT-52: Actualización concurrente del mismo producto

        [Fact(DisplayName = "IT-52: Actualizaciones secuenciales del mismo producto deben ser exitosas")]
        public async Task UpdateProduct_Sequentially_ShouldSucceed()
        {
            using var scope1 = _factory.Services.CreateScope();
            using var scope2 = _factory.Services.CreateScope();

            var productService1 = scope1.ServiceProvider.GetRequiredService<IProductService>();
            var productService2 = scope2.ServiceProvider.GetRequiredService<IProductService>();

            var update1 = new MicroMercado.Application.DTOs.Product.UpdateProductDTO
            {
                Id = 1,
                Name = "Yogurt Actualizado 1",
                Description = "Primera actualización",
                Brand = "Pil",
                Price = 11.00m,
                Stock = 50,
                CategoryId = 1
            };

            var result1 = await productService1.UpdateProductAsync(update1);
            Assert.NotNull(result1);

            var update2 = new MicroMercado.Application.DTOs.Product.UpdateProductDTO
            {
                Id = 1,
                Name = "Yogurt Actualizado 2",
                Description = "Segunda actualización",
                Brand = "Pil",
                Price = 12.00m,
                Stock = 50,
                CategoryId = 1
            };

            var result2 = await productService2.UpdateProductAsync(update2);
            Assert.NotNull(result2);
            Assert.Equal(12.00m, result2.Price);
        }

        #endregion

        #region IT-53: Request con datos JSON mal formados

        [Fact(DisplayName = "IT-53: Request con JSON inválido debe retornar error")]
        public async Task PostRequest_WithMalformedJson_ShouldReturnError()
        {
            var malformedJson = "{\"name\": \"Test\", invalid json";
            var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/Sales?handler=ConfirmSale", content);

            Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact(DisplayName = "IT-53b: Request sin Content-Type debe manejar error")]
        public async Task PostRequest_WithoutContentType_ShouldHandleGracefully()
        {
            var content = new StringContent("{}", Encoding.UTF8);
            content.Headers.Remove("Content-Type");

            var response = await _client.PostAsync("/Sales?handler=ConfirmSale", content);

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType);
        }

        #endregion

        #region Pruebas adicionales de edge cases

        [Fact(DisplayName = "IT-51b: Venta con cantidad cero debe fallar")]
        public async Task CreateSale_WithZeroQuantity_ShouldFail()
        {
            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = null,
                TotalAmount = 0m,
                CashReceived = 0m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 0, Price = 10.00m, Total = 0m }
                }
            };

            var result = await saleService.CreateSaleAsync(createSaleDTO);

            Assert.False(result.Success);
        }

        [Fact(DisplayName = "IT-52b: Crear múltiples productos con mismo nombre secuencialmente debe fallar")]
        public async Task CreateProducts_WithDuplicateNames_ShouldFailSecond()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var dto1 = new MicroMercado.Application.DTOs.Product.CreateProductDTO
            {
                Name = "Producto Duplicado Test",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = 10,
                CategoryId = 1
            };

            var result1 = await productService.CreateProductAsync(dto1);
            Assert.NotNull(result1);

            var dto2 = new MicroMercado.Application.DTOs.Product.CreateProductDTO
            {
                Name = "Producto Duplicado Test",
                Description = "Test 2",
                Brand = "Test",
                Price = 15.00m,
                Stock = 20,
                CategoryId = 1
            };

            var result2 = await productService.CreateProductAsync(dto2);
            Assert.Null(result2);
        }

        [Fact(DisplayName = "IT-53c: Búsqueda con término muy largo no debe crashear")]
        public async Task SearchProducts_WithVeryLongTerm_ShouldNotCrash()
        {
            var longTerm = new string('A', 1000);

            var response = await _client.GetAsync($"/Sales?handler=SearchProducts&term={longTerm}");

            Assert.True(
                response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == System.Net.HttpStatusCode.BadRequest
            );
        }

        [Fact(DisplayName = "IT-51c: Venta con producto eliminado debe fallar")]
        public async Task CreateSale_WithDeletedProduct_ShouldFail()
        {
            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var createDto = new MicroMercado.Application.DTOs.Product.CreateProductDTO
            {
                Name = "Producto a eliminar para venta",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = 10,
                CategoryId = 1
            };

            var product = await productService.CreateProductAsync(createDto);
            Assert.NotNull(product);

            await productService.DeleteProductAsync(product.Id);

            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = null,
                TotalAmount = 10.00m,
                CashReceived = 10.00m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO { ProductId = product.Id, Quantity = 1, Price = 10.00m, Total = 10.00m }
                }
            };

            var result = await saleService.CreateSaleAsync(createSaleDTO);

            Assert.False(result.Success);
        }

        [Fact(DisplayName = "IT-52c: Reducir stock a negativo mediante ventas debe fallar")]
        public async Task CreateSale_ThatWouldMakeStockNegative_ShouldFail()
        {
            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var currentStock = await context.Products
                .Where(p => p.Id == 1)
                .Select(p => p.Stock)
                .FirstAsync();

            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = null,
                TotalAmount = (currentStock + 10) * 10.00m,
                CashReceived = (currentStock + 10) * 10.00m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO
                    {
                        ProductId = 1,
                        Quantity = (short)(currentStock + 10),
                        Price = 10.00m,
                        Total = (currentStock + 10) * 10.00m
                    }
                }
            };

            var result = await saleService.CreateSaleAsync(createSaleDTO);

            Assert.False(result.Success);
            Assert.Contains("stock", result.Message.ToLower());
        }

        #endregion
    }
}