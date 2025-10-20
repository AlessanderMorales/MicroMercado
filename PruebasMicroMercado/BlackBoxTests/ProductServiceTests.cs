using FluentValidation;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using MicroMercado.Domain.Entities;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.WhiteBoxTests
{
    public class ProductServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly Mock<IValidator<CreateProductDTO>> _createValidatorMock;
        private readonly Mock<IValidator<UpdateProductDTO>> _updateValidatorMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<ProductService>>();
            _createValidatorMock = new Mock<IValidator<CreateProductDTO>>();
            _updateValidatorMock = new Mock<IValidator<UpdateProductDTO>>();

            // Setup validators to always return valid by default
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<CreateProductDTO>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateProductDTO>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _productService = new ProductService(
                _context,
                _loggerMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object
            );

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var category = new Category
            {
                Id = 1,
                Name = "Test Category",
                Description = "Test Category Description",
                Status = 1,
                CreatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            _context.SaveChanges();

            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Test Product 1",
                    Description = "Description 1",
                    Brand = "Brand A",
                    Price = 10.50m,
                    Stock = 100,
                    CategoryId = 1,
                    Status = 1,
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 2,
                    Name = "Test Product 2",
                    Description = "Description 2",
                    Brand = "Brand B",
                    Price = 20.00m,
                    Stock = 50,
                    CategoryId = 1,
                    Status = 1,
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 3,
                    Name = "Out of Stock Product",
                    Description = "No stock",
                    Brand = "Brand C",
                    Price = 15.00m,
                    Stock = 0,
                    CategoryId = 1,
                    Status = 1,
                    CreatedAt = DateTime.Now
                }
            };

            _context.Products.AddRange(products);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetProductByIdAsync_ExistingProduct_ShouldReturnProduct()
        {
            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Product 1", result.Name);
            Assert.Equal(10.50m, result.Price);
        }

        [Fact]
        public async Task GetProductByIdAsync_NonExistingProduct_ShouldReturnNull()
        {
            // Act
            var result = await _productService.GetProductByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SearchProductsAsync_ByName_ShouldReturnMatchingProducts()
        {
            // Act
            var result = await _productService.SearchProductsAsync("Product 1");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, p => p.Name.Contains("Product 1"));
        }

        [Fact]
        public async Task SearchProductsAsync_ByBrand_ShouldReturnMatchingProducts()
        {
            // Act
            var result = await _productService.SearchProductsAsync("Brand B");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, p => p.Brand == "Brand B");
        }

        [Fact]
        public async Task HasStockAsync_ProductWithStock_ShouldReturnTrue()
        {
            // Act
            var result = await _productService.HasStockAsync(1, 10);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasStockAsync_ProductWithoutStock_ShouldReturnFalse()
        {
            // Act
            var result = await _productService.HasStockAsync(3, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasStockAsync_RequestedQuantityExceedsStock_ShouldReturnFalse()
        {
            // Act
            var result = await _productService.HasStockAsync(1, 200);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}