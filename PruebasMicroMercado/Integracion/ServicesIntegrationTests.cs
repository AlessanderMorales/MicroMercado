using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using MicroMercado.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.Integracion
{
 
    [Collection("IntegrationTests")]
    public class ServicesIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ServicesIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        #region IT-16 a IT-18: Product Service Integration Tests

        [Fact(DisplayName = "IT-16: ProductService - Crear producto con categoría válida")]
        public async Task ProductService_CreateProduct_WithValidCategory_ShouldSucceed()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var newProduct = new CreateProductDTO
            {
                Name = "Producto Test Integration",
                Description = "Descripción de prueba",
                Brand = "Marca Test",
                Price = 15.50m,
                Stock = 25,
                CategoryId = 1
            };

            var result = await productService.CreateProductAsync(newProduct);

            Assert.NotNull(result);
            Assert.Equal("Producto Test Integration", result.Name);
            Assert.Equal(15.50m, result.Price);
        }

        [Fact(DisplayName = "IT-17: ProductService - Buscar productos existentes")]
        public async Task ProductService_SearchProducts_ShouldReturnResults()
        {
            _factory.SeedDatabase();
            
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var results = await productService.SearchProductsAsync("Yogurt");

            Assert.NotNull(results);
            var productList = results.ToList();
            Assert.NotEmpty(productList);
            Assert.Contains(productList, p => p.Name.Contains("Yogurt"));
        }

        [Fact(DisplayName = "IT-18: ProductService - Verificar stock de producto")]
        public async Task ProductService_CheckStock_ShouldReturnCorrectAvailability()
        {
            _factory.SeedDatabase();
            
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var hasStock = await productService.HasStockAsync(1, 10);
            Assert.True(hasStock);

            var hasInsufficientStock = await productService.HasStockAsync(1, 100);
            Assert.False(hasInsufficientStock);
        }

        #endregion

        #region IT-19 a IT-20: Client Service Integration Tests

        [Fact(DisplayName = "IT-19: ClientService - Buscar cliente por documento")]
        public async Task ClientService_SearchByDocument_ShouldReturnClient()
        {
            _factory.SeedDatabase();
            
            using var scope = _factory.Services.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            var client = await clientService.GetClientByTaxDocumentAsync("12345678");

            Assert.NotNull(client);
            Assert.Equal("Cliente Test", client.BusinessName);
            Assert.Equal("test@email.com", client.Email);
        }

        [Fact(DisplayName = "IT-20: ClientService - Cliente inexistente debe retornar null")]
        public async Task ClientService_SearchNonExistent_ShouldReturnNull()
        {
            using var scope = _factory.Services.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            var client = await clientService.GetClientByTaxDocumentAsync("99999999");

            Assert.Null(client);
        }

        #endregion

        #region IT-21 a IT-22: Category Service Integration Tests

        [Fact(DisplayName = "IT-21: CategoryService - Obtener todas las categorías activas")]
        public async Task CategoryService_GetAllActive_ShouldReturnCategories()
        {
            _factory.SeedDatabase();
            
            using var scope = _factory.Services.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

            var categories = await categoryService.GetAllCategoriesAsync();

            Assert.NotNull(categories);
            var categoryList = categories.ToList();
            Assert.NotEmpty(categoryList);
            Assert.Contains(categoryList, c => c.Name == "Lácteos");
            Assert.Contains(categoryList, c => c.Name == "Alimentos");
        }

        [Fact(DisplayName = "IT-22: CategoryService - Obtener categoría por ID")]
        public async Task CategoryService_GetById_ShouldReturnCategory()
        {
            _factory.SeedDatabase();
            
            using var scope = _factory.Services.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

            var category = await categoryService.GetCategoryByIdAsync(1);

            Assert.NotNull(category);
            Assert.Equal("Lácteos", category.Name);
        }

        #endregion

        #region IT-23: Database Context Integration Test

        [Fact(DisplayName = "IT-23: ApplicationDbContext - Verificar conexión y datos iniciales")]
        public void DbContext_ShouldContainSeedData()
        {
            _factory.SeedDatabase();
            
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Assert.True(context.Categories.Count() >= 2);
            Assert.True(context.Products.Count() >= 2);
            Assert.True(context.Clients.Count() >= 1);
        }

        #endregion

        #region IT-24 a IT-25: Validación con FluentValidation Integration

        [Fact(DisplayName = "IT-24: ProductService - Rechazar precio negativo")]
        public async Task ProductService_WithValidation_ShouldRejectNegativePrice()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var invalidProduct = new CreateProductDTO
            {
                Name = "Producto Inválido",
                Description = "Test",
                Brand = "Test",
                Price = -10.00m,
                Stock = 5,
                CategoryId = 1
            };

            var result = await productService.CreateProductAsync(invalidProduct);
            Assert.Null(result);
        }

        [Fact(DisplayName = "IT-25: ProductService - Rechazar stock negativo")]
        public async Task ProductService_WithValidation_ShouldRejectNegativeStock()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var invalidProduct = new CreateProductDTO
            {
                Name = "Producto Stock Negativo",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = -5,
                CategoryId = 1
            };

            var result = await productService.CreateProductAsync(invalidProduct);
            Assert.Null(result);
        }

        #endregion
    }
}
