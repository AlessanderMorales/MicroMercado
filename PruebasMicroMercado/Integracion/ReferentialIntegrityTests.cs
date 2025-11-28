using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Application.Services;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.Integracion
{
    [Collection("IntegrationTests")]
 public class ReferentialIntegrityTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
  private readonly CustomWebApplicationFactory<Program> _factory;

        public ReferentialIntegrityTests(CustomWebApplicationFactory<Program> factory)
        {
_factory = factory;
     }

 #region IT-40: No se puede eliminar categoría con productos asociados

     [Fact(DisplayName = "IT-40: Eliminar categoría realiza borrado lógico")]
        public async Task DeleteCategory_WithAssociatedProducts_ShouldPerformLogicalDelete()
        {
            _factory.SeedDatabase();

            using var scope = _factory.Services.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Arrange - Crear categoría y producto asociado
            var categoryDto = new CreateCategoryDTO
            {
                Name = "Cat IT40",  // Máximo 20 caracteres
                Description = "Categoria Test"
            };

            var category = await categoryService.CreateCategoryAsync(categoryDto);
            Assert.NotNull(category);

            var productDto = new CreateProductDTO
            {
                Name = "Prod IT40",
                Description = "Test",
                Brand = "Test",
                Price = 10.00m,
                Stock = 10,
                CategoryId = category.Id
            };

            var product = await productService.CreateProductAsync(productDto);
            Assert.NotNull(product);

            // Act - Eliminar la categoría (borrado lógico)
            var result = await categoryService.DeleteCategoryAsync(category.Id);

            // Assert - La categoría se marca como inactiva
            Assert.True(result);
            
            var categoryInDb = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == category.Id);
            Assert.NotNull(categoryInDb);
            Assert.Equal((byte)0, categoryInDb.Status);
        }

        #endregion

        #region IT-41: No se puede eliminar producto que está en una venta

   [Fact(DisplayName = "IT-41: Eliminar producto realiza borrado lógico")]
        public async Task DeleteProduct_WithAssociatedSales_ShouldPerformLogicalDelete()
        {
            _factory.SeedDatabase();

            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Arrange - Crear una venta con el producto 1
            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = null,
                TotalAmount = 10.00m,
                CashReceived = 10.00m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 10.00m, Total = 10.00m }
                }
            };

            var saleResult = await saleService.CreateSaleAsync(createSaleDTO);
            Assert.True(saleResult.Success);

            // Act - Eliminar el producto (borrado lógico)
            var result = await productService.DeleteProductAsync(1);

            // Assert - El producto se marca como inactivo
            Assert.True(result);
            
            var productInDb = await context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == 1);
            Assert.NotNull(productInDb);
            Assert.Equal((byte)0, productInDb.Status);
        }

        #endregion

        #region IT-42: No se puede eliminar cliente que tiene ventas asociadas

        [Fact(DisplayName = "IT-42: Eliminar cliente realiza borrado lógico")]
        public async Task DeleteClient_WithAssociatedSales_ShouldPerformLogicalDelete()
        {
            _factory.SeedDatabase();

            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Arrange - Crear una venta asociada al cliente 1
            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
                ClientId = 1,
                TotalAmount = 10.00m,
                CashReceived = 10.00m,
                Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
                {
                    new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 10.00m, Total = 10.00m }
                }
            };

            var saleResult = await saleService.CreateSaleAsync(createSaleDTO);
            Assert.True(saleResult.Success);

            // Act - Eliminar el cliente (borrado lógico)
            var result = await clientService.DeleteClientAsync(1);

            // Assert - El cliente se marca como inactivo
            Assert.True(result);
            
            var clientInDb = await context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == 1);
            Assert.NotNull(clientInDb);
            Assert.Equal((byte)0, clientInDb.Status);
        }

        #endregion

        #region Pruebas adicionales de integridad

        [Fact(DisplayName = "IT-40b: Eliminar categoría sin productos realiza borrado lógico")]
        public async Task DeleteCategory_WithoutAssociatedProducts_ShouldSucceed()
        {
            _factory.SeedDatabase();

            using var scope = _factory.Services.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Arrange - Crear categoría sin productos
            var categoryDto = new CreateCategoryDTO
            {
                Name = "Cat IT40b",  // Máximo 20 caracteres
                Description = "Categoria sin productos"
            };

            var category = await categoryService.CreateCategoryAsync(categoryDto);
            Assert.NotNull(category);

            // Act
            var result = await categoryService.DeleteCategoryAsync(category.Id);

            // Assert - Borrado lógico
            Assert.True(result);

            var categoryInDb = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == category.Id);
            Assert.NotNull(categoryInDb);
            Assert.Equal((byte)0, categoryInDb.Status);
        }

        [Fact(DisplayName = "IT-41b: Actualizar producto que está en venta debe ser permitido")]
        public async Task UpdateProduct_WithAssociatedSales_ShouldBeAllowed()
        {
            _factory.SeedDatabase();

            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

 // Arrange - Crear venta con producto 1
    var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
            ClientId = null,
       TotalAmount = 10.00m,
         CashReceived = 10.00m,
     Change = 0m,
  Items = new List<SaleDTO.SaleItemDTO>
  {
    new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 10.00m, Total = 10.00m }
    }
            };

            await saleService.CreateSaleAsync(createSaleDTO);

            // Act - Actualizar el producto (debería permitirse)
       var updateDto = new UpdateProductDTO
 {
     Id = 1,
          Name = "Yogurt Natural Actualizado",
  Description = "Actualizado después de venta",
    Brand = "Pil",
         Price = 12.00m,
    Stock = 45, // Stock ya fue reducido por la venta
           CategoryId = 1
  };

       var result = await productService.UpdateProductAsync(updateDto);

            // Assert
            Assert.NotNull(result);
       Assert.Equal("Yogurt Natural Actualizado", result.Name);
 }

        [Fact(DisplayName = "IT-42b: Actualizar cliente que tiene ventas debe ser permitido")]
        public async Task UpdateClient_WithAssociatedSales_ShouldBeAllowed()
        {
            _factory.SeedDatabase();

            using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            // Arrange - Crear venta con cliente 1
       var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
      ClientId = 1,
    TotalAmount = 10.00m,
          CashReceived = 10.00m,
          Change = 0m,
 Items = new List<SaleDTO.SaleItemDTO>
    {
        new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 10.00m, Total = 10.00m }
        }
            };

     await saleService.CreateSaleAsync(createSaleDTO);

            // Act - Actualizar el cliente (debería permitirse)
 var updateDto = new UpdateClientDTO
            {
  Id = 1,
       BusinessName = "Cliente Test Actualizado",
       Email = "actualizado@test.com",
                TaxDocument = "12345678",
  Address = "Nueva dirección",
          Status = 1
            };

var result = await clientService.UpdateClientAsync(updateDto);

     // Assert
  Assert.NotNull(result);
            Assert.Equal("Cliente Test Actualizado", result.BusinessName);
     }

        #endregion
    }
}
