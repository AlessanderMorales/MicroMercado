using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MicroMercado.Services;
using MicroMercado.Pages;
using MicroMercado.DTOs.Sales;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace PruebasMicroMercado
{
    public class UnitTest1
    {
        [Fact]
        public async Task OnGetSearchProductsAsync_ReturnsFailure_WhenTermEmpty()
        {
            var mockService = new Mock<IProductService>();
            var logger = new NullLogger<SalesModel>();
            var page = new SalesModel(mockService.Object, logger);

            var result = await page.OnGetSearchProductsAsync("   ") as JsonResult;

            Assert.NotNull(result);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal("Término de búsqueda vacío", doc.RootElement.GetProperty("message").GetString());

            mockService.Verify(s => s.SearchProductsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task OnGetSearchProductsAsync_ReturnsProducts_WhenTermProvided()
        {
            var products = new List<ProductSearchDTO>
            {
                new ProductSearchDTO { Id = 1, Name = "Arroz", Brand = "MarcaA", Description = "Arroz 1kg", Price = 10m, Stock = 5, CategoryId = 1, CategoryName = "Alimentos" }
            };

            var mockService = new Mock<IProductService>();
            mockService.Setup(s => s.SearchProductsAsync(It.Is<string>(t => t == "Arroz")))
                .ReturnsAsync(products);

            var logger = new NullLogger<SalesModel>();
            var page = new SalesModel(mockService.Object, logger);

            var result = await page.OnGetSearchProductsAsync("Arroz") as JsonResult;

            Assert.NotNull(result);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());

            var data = doc.RootElement.GetProperty("data");
            Assert.True(data.ValueKind == JsonValueKind.Array);
            Assert.Equal(1, data.GetArrayLength());

            var first = data[0];
            Assert.Equal(1, first.GetProperty("id").GetInt32());
            Assert.Equal("Arroz", first.GetProperty("name").GetString());
            Assert.Equal("MarcaA", first.GetProperty("brand").GetString());

            mockService.Verify(s => s.SearchProductsAsync("Arroz"), Times.Once);
        }

        [Fact]
        public async Task OnGetProductByIdAsync_ReturnsNotFound_WhenNull()
        {
            var mockService = new Mock<IProductService>();
            mockService.Setup(s => s.GetProductByIdAsync(It.IsAny<short>()))
                .ReturnsAsync((ProductSearchDTO?)null);

            var logger = new NullLogger<SalesModel>();
            var page = new SalesModel(mockService.Object, logger);

            var result = await page.OnGetProductByIdAsync(99) as JsonResult;

            Assert.NotNull(result);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal("Producto no encontrado", doc.RootElement.GetProperty("message").GetString());

            mockService.Verify(s => s.GetProductByIdAsync(99), Times.Once);
        }

        [Fact]
        public async Task OnGetProductByIdAsync_ReturnsProduct_WhenFound()
        {
            var dto = new ProductSearchDTO { Id = 1, Name = "Arroz", Brand = "MarcaA", Description = "Arroz 1kg", Price = 10m, Stock = 5, CategoryId = 1, CategoryName = "Alimentos" };

            var mockService = new Mock<IProductService>();
            mockService.Setup(s => s.GetProductByIdAsync((short)1))
                .ReturnsAsync(dto);

            var logger = new NullLogger<SalesModel>();
            var page = new SalesModel(mockService.Object, logger);

            var result = await page.OnGetProductByIdAsync(1) as JsonResult;

            Assert.NotNull(result);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());

            var data = doc.RootElement.GetProperty("data");
            Assert.Equal(1, data.GetProperty("id").GetInt32());
            Assert.Equal("Arroz", data.GetProperty("name").GetString());

            mockService.Verify(s => s.GetProductByIdAsync(1), Times.Once);
        }

        [Theory]
        [InlineData((short)1, (short)5, true)]
        [InlineData((short)1, (short)10, false)]
        public async Task OnGetCheckStockAsync_ReturnsCorrectHasStock(short productId, short quantity, bool expected)
        {
            var mockService = new Mock<IProductService>();
            mockService.Setup(s => s.HasStockAsync(productId, quantity)).ReturnsAsync(expected);

            var logger = new NullLogger<SalesModel>();
            var page = new SalesModel(mockService.Object, logger);

            var result = await page.OnGetCheckStockAsync(productId, quantity) as JsonResult;

            Assert.NotNull(result);
            var doc = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal(expected, doc.RootElement.GetProperty("hasStock").GetBoolean());

            mockService.Verify(s => s.HasStockAsync(productId, quantity), Times.Once);
        }
    }
}