using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.Integracion
{
    [Collection("IntegrationTests")]
    public class CrudIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CrudIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region IT-31 a IT-34: Actualización y eliminación de productos

        [Fact(DisplayName = "IT-31: Actualizar producto existente debe ser exitoso")]
        public async Task UpdateProduct_WithValidData_ShouldSucceed()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _factory.SeedDatabase();

            var createDto = new CreateProductDTO
            {
                Name = "Producto para actualizar",
                Description = "Descripción original",
                Brand = "Marca Original",
                Price = 10.00m,
                Stock = 50,
                CategoryId = 1
            };

            var createdProduct = await productService.CreateProductAsync(createDto);
            Assert.NotNull(createdProduct);

            var updateDto = new UpdateProductDTO
            {
                Id = createdProduct.Id,
                Name = "Producto Actualizado",
                Description = "Descripción actualizada",
                Brand = "Marca Actualizada",
                Price = 15.00m,
                Stock = 60,
                CategoryId = 1
            };

            var updatedProduct = await productService.UpdateProductAsync(updateDto);

            Assert.NotNull(updatedProduct);
            Assert.Equal("Producto Actualizado", updatedProduct.Name);
            Assert.Equal(15.00m, updatedProduct.Price);
            Assert.Equal(60, updatedProduct.Stock);

            var productInDb = await context.Products.FindAsync(createdProduct.Id);
            Assert.Equal("Producto Actualizado", productInDb.Name);
        }

        [Fact(DisplayName = "IT-32: Actualizar producto con nombre duplicado debe fallar")]
        public async Task UpdateProduct_WithDuplicateName_ShouldFail()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            _factory.SeedDatabase();

            var updateDto = new UpdateProductDTO
            {
                Id = 2,
                Name = "Yogurt Natural",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = 10,
                CategoryId = 1
            };

            var result = await productService.UpdateProductAsync(updateDto);

            Assert.Null(result);
        }

        [Fact(DisplayName = "IT-33: Actualizar producto con categoría inactiva debe fallar")]
        public async Task UpdateProduct_WithInactiveCategory_ShouldFail()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _factory.SeedDatabase();

            var inactiveCategory = new MicroMercado.Domain.Models.Category
            {
                Name = "Categoría Inactiva Test",
                Status = 0,
                LastUpdate = System.DateTime.Now
            };
            context.Categories.Add(inactiveCategory);
            await context.SaveChangesAsync();

            var updateDto = new UpdateProductDTO
            {
                Id = 1,
                Name = "Producto Test",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = 10,
                CategoryId = inactiveCategory.Id
            };

            var result = await productService.UpdateProductAsync(updateDto);

            Assert.Null(result);
        }

        [Fact(DisplayName = "IT-34: Eliminar producto debe realizar eliminación lógica")]
        public async Task DeleteProduct_ShouldPerformLogicalDelete()
        {
            using var scope = _factory.Services.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _factory.SeedDatabase();

            var createDto = new CreateProductDTO
            {
                Name = "Producto a eliminar IT34",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = 10,
                CategoryId = 1
            };

            var createdProduct = await productService.CreateProductAsync(createDto);
            Assert.NotNull(createdProduct);

            var result = await productService.DeleteProductAsync(createdProduct.Id);

            Assert.True(result);

            var productInDb = await context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == createdProduct.Id);
            Assert.NotNull(productInDb);
            Assert.Equal((byte)0, productInDb.Status);
        }

        #endregion

        #region IT-35 a IT-37: Actualización y eliminación de clientes

        [Fact(DisplayName = "IT-35: Actualizar cliente existente debe ser exitoso")]
        public async Task UpdateClient_WithValidData_ShouldSucceed()
        {
            using var scope = _factory.Services.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            _factory.SeedDatabase();

            var updateDto = new UpdateClientDTO
            {
                Id = 1,
                BusinessName = "Cliente Actualizado",
                Email = "actualizado@test.com",
                Address = "Nueva Dirección 456",
                TaxDocument = "12345678",
                Status = 1
            };

            var result = await clientService.UpdateClientAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal("Cliente Actualizado", result.BusinessName);
            Assert.Equal("actualizado@test.com", result.Email);
        }

        [Fact(DisplayName = "IT-36: Actualizar cliente con documento duplicado debe fallar")]
        public async Task UpdateClient_WithDuplicateTaxDocument_ShouldFail()
        {
            using var scope = _factory.Services.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _factory.SeedDatabase();

            var client2 = new MicroMercado.Domain.Models.Client
            {
                BusinessName = "Cliente 2",
                Email = "cliente2@test.com",
                TaxDocument = "87654321",
                Status = 1,
                LastUpdate = System.DateTime.Now
            };
            context.Clients.Add(client2);
            await context.SaveChangesAsync();

            var updateDto = new UpdateClientDTO
            {
                Id = client2.Id,
                BusinessName = "Cliente 2 Actualizado",
                Email = "cliente2@test.com",
                TaxDocument = "12345678",
                Address = "Test",
                Status = 1
            };

            var result = await clientService.UpdateClientAsync(updateDto);

            Assert.Null(result);
        }

        [Fact(DisplayName = "IT-37: Eliminar cliente debe realizar eliminación lógica")]
        public async Task DeleteClient_ShouldPerformLogicalDelete()
        {
            using var scope = _factory.Services.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _factory.SeedDatabase();

            var createDto = new CreateClientDTO
            {
                BusinessName = "Cliente IT37",
                Email = "eliminar37@test.com",
                TaxDocument = "1111137",  // 7 dígitos válidos (6-8 permitidos)
                Address = "Calle Test 123"  // Mínimo 5 caracteres
            };

            var createdClient = await clientService.CreateClientAsync(createDto);
            Assert.NotNull(createdClient);

            var result = await clientService.DeleteClientAsync(createdClient.Id);

            Assert.True(result);

            var clientInDb = await context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == createdClient.Id);
            Assert.NotNull(clientInDb);
            Assert.Equal((byte)0, clientInDb.Status);
        }

        #endregion

        #region IT-38 a IT-39: Actualización y eliminación de categorías

        [Fact(DisplayName = "IT-38: Actualizar categoría existente debe ser exitoso")]
        public async Task UpdateCategory_WithValidData_ShouldSucceed()
        {
            using var scope = _factory.Services.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

            _factory.SeedDatabase();

            var updateDto = new UpdateCategoryDTO
            {
                Id = 1,
                Name = "Lácteos Actualizados",
                Description = "Descripción actualizada",
                Status = 1
            };

            var result = await categoryService.UpdateCategoryAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal("Lácteos Actualizados", result.Name);
        }

        [Fact(DisplayName = "IT-39: Eliminar categoría debe realizar eliminación lógica")]
        public async Task DeleteCategory_ShouldPerformLogicalDelete()
        {
            using var scope = _factory.Services.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _factory.SeedDatabase();

            var createDto = new CreateCategoryDTO
            {
                Name = "Cat IT39",  // Máximo 20 caracteres, solo alfanuméricos
                Description = "Categoria Test"  // Máximo 80 caracteres
            };

            var createdCategory = await categoryService.CreateCategoryAsync(createDto);
            Assert.NotNull(createdCategory);

            var result = await categoryService.DeleteCategoryAsync(createdCategory.Id);

            Assert.True(result);

            var categoryInDb = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == createdCategory.Id);
            Assert.NotNull(categoryInDb);
            Assert.Equal((byte)0, categoryInDb.Status);
        }

        #endregion
    }
}
