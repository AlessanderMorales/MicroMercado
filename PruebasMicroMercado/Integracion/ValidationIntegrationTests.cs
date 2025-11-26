using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.Integracion
{
    [Collection("IntegrationTests")]
  public class ValidationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
 private readonly CustomWebApplicationFactory<Program> _factory;

        public ValidationIntegrationTests(CustomWebApplicationFactory<Program> factory)
   {
      _factory = factory;
        }

        #region IT-48: Validación de productos con datos inválidos

   [Fact(DisplayName = "IT-48a: Crear producto con precio negativo debe fallar")]
        public async Task CreateProduct_WithNegativePrice_ShouldFail()
    {
   using var scope = _factory.Services.CreateScope();
       var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

       var invalidProduct = new CreateProductDTO
         {
    Name = "Producto Inválido",
  Description = "Test",
                Brand = "Test",
      Price = -10.00m, // Precio negativo
  Stock = 10,
      CategoryId = 1
   };

     var result = await productService.CreateProductAsync(invalidProduct);

  Assert.Null(result); // Debe fallar por validación
        }

   [Fact(DisplayName = "IT-48b: Crear producto con stock negativo debe fallar")]
        public async Task CreateProduct_WithNegativeStock_ShouldFail()
   {
      using var scope = _factory.Services.CreateScope();
  var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var invalidProduct = new CreateProductDTO
         {
   Name = "Producto Inválido",
      Description = "Test",
    Brand = "Test",
      Price = 10.00m,
    Stock = -5, // Stock negativo
      CategoryId = 1
  };

        var result = await productService.CreateProductAsync(invalidProduct);

      Assert.Null(result);
   }

    [Fact(DisplayName = "IT-48c: Crear producto con precio excesivo debe fallar")]
        public async Task CreateProduct_WithExcessivePrice_ShouldFail()
        {
     using var scope = _factory.Services.CreateScope();
       var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

    var invalidProduct = new CreateProductDTO
  {
    Name = "Producto Inválido",
       Description = "Test",
Brand = "Test",
       Price = 99999.99m, // Precio excesivo (límite es 9999.99)
    Stock = 10,
     CategoryId = 1
   };

       var result = await productService.CreateProductAsync(invalidProduct);

  Assert.Null(result);
   }

      [Fact(DisplayName = "IT-48d: Crear producto sin nombre debe fallar")]
        public async Task CreateProduct_WithoutName_ShouldFail()
   {
   using var scope = _factory.Services.CreateScope();
       var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

       var invalidProduct = new CreateProductDTO
   {
     Name = "", // Nombre vacío
   Description = "Test",
     Brand = "Test",
    Price = 10.00m,
        Stock = 10,
       CategoryId = 1
   };

 var result = await productService.CreateProductAsync(invalidProduct);

   Assert.Null(result);
  }

        [Fact(DisplayName = "IT-48e: Crear producto con nombre muy largo debe fallar")]
     public async Task CreateProduct_WithTooLongName_ShouldFail()
   {
      using var scope = _factory.Services.CreateScope();
 var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

   var invalidProduct = new CreateProductDTO
     {
        Name = new string('A', 51), // Nombre de 51 caracteres (límite es 50)
    Description = "Test",
     Brand = "Test",
      Price = 10.00m,
     Stock = 10,
     CategoryId = 1
 };

    var result = await productService.CreateProductAsync(invalidProduct);

        Assert.Null(result);
}

        #endregion

        #region IT-49: Validación de clientes con datos inválidos

   [Fact(DisplayName = "IT-49a: Crear cliente sin email debe fallar")]
  public async Task CreateClient_WithoutEmail_ShouldFail()
   {
    using var scope = _factory.Services.CreateScope();
       var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            var invalidClient = new CreateClientDTO
            {
 BusinessName = "Cliente Test",
    Email = "", // Email vacío
                TaxDocument = "12345678",
     Address = "Test"
   };

      var result = await clientService.CreateClientAsync(invalidClient);

   Assert.Null(result);
      }

   [Fact(DisplayName = "IT-49b: Crear cliente con email inválido debe fallar")]
        public async Task CreateClient_WithInvalidEmail_ShouldFail()
     {
   using var scope = _factory.Services.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

       var invalidClient = new CreateClientDTO
 {
      BusinessName = "Cliente Test",
        Email = "email-invalido", // Email sin formato correcto
TaxDocument = "12345678",
       Address = "Test"
       };

  var result = await clientService.CreateClientAsync(invalidClient);

       Assert.Null(result);
    }

  [Fact(DisplayName = "IT-49c: Crear cliente con documento solo letras debe fallar")]
        public async Task CreateClient_WithNonNumericTaxDocument_ShouldFail()
{
       using var scope = _factory.Services.CreateScope();
   var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

      var invalidClient = new CreateClientDTO
       {
       BusinessName = "Cliente Test",
   Email = "test@test.com",
TaxDocument = "ABCD1234", // Documento con letras
       Address = "Test"
       };

            var result = await clientService.CreateClientAsync(invalidClient);

    Assert.Null(result);
 }

        [Fact(DisplayName = "IT-49d: Crear cliente sin nombre debe fallar")]
      public async Task CreateClient_WithoutBusinessName_ShouldFail()
 {
    using var scope = _factory.Services.CreateScope();
        var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

            var invalidClient = new CreateClientDTO
    {
   BusinessName = "", // Nombre vacío
       Email = "test@test.com",
        TaxDocument = "12345678",
        Address = "Test"
     };

     var result = await clientService.CreateClientAsync(invalidClient);

            Assert.Null(result);
        }

  [Fact(DisplayName = "IT-49e: Crear cliente con documento muy largo debe fallar")]
  public async Task CreateClient_WithTooLongTaxDocument_ShouldFail()
        {
    using var scope = _factory.Services.CreateScope();
  var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

       var invalidClient = new CreateClientDTO
   {
     BusinessName = "Cliente Test",
    Email = "test@test.com",
     TaxDocument = "123456789012345678901", // Documento de 21 dígitos (límite es 20)
        Address = "Test"
    };

var result = await clientService.CreateClientAsync(invalidClient);

       Assert.Null(result);
   }

        #endregion

  #region IT-50: Validación de categorías con datos inválidos

   [Fact(DisplayName = "IT-50a: Crear categoría sin nombre debe fallar")]
        public async Task CreateCategory_WithoutName_ShouldFail()
        {
  using var scope = _factory.Services.CreateScope();
      var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

 var invalidCategory = new CreateCategoryDTO
 {
          Name = "", // Nombre vacío
Description = "Test"
 };

      var result = await categoryService.CreateCategoryAsync(invalidCategory);

            Assert.Null(result);
  }

        [Fact(DisplayName = "IT-50b: Crear categoría con nombre muy largo debe fallar")]
public async Task CreateCategory_WithTooLongName_ShouldFail()
     {
  using var scope = _factory.Services.CreateScope();
  var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

            var invalidCategory = new CreateCategoryDTO
   {
        Name = new string('A', 21), // Nombre de 21 caracteres (límite es 20)
       Description = "Test"
       };

        var result = await categoryService.CreateCategoryAsync(invalidCategory);

Assert.Null(result);
}

   [Fact(DisplayName = "IT-50c: Crear categoría con nombre duplicado debe fallar")]
        public async Task CreateCategory_WithDuplicateName_ShouldFail()
        {
        using var scope = _factory.Services.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

      var invalidCategory = new CreateCategoryDTO
     {
       Name = "Lácteos", // Nombre duplicado del seed
       Description = "Test"
};

   var result = await categoryService.CreateCategoryAsync(invalidCategory);

            Assert.Null(result);
        }

        #endregion

    #region Pruebas de validación de actualización

     [Fact(DisplayName = "IT-48f: Actualizar producto con precio negativo debe fallar")]
     public async Task UpdateProduct_WithNegativePrice_ShouldFail()
   {
using var scope = _factory.Services.CreateScope();
       var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

        var invalidUpdate = new UpdateProductDTO
       {
   Id = 1,
    Name = "Test",
      Description = "Test",
 Brand = "Test",
 Price = -10.00m, // Precio negativo
            Stock = 10,
   CategoryId = 1
     };

       var result = await productService.UpdateProductAsync(invalidUpdate);

Assert.Null(result);
   }

        [Fact(DisplayName = "IT-49f: Actualizar cliente con email inválido debe fallar")]
 public async Task UpdateClient_WithInvalidEmail_ShouldFail()
        {
    using var scope = _factory.Services.CreateScope();
            var clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

       var invalidUpdate = new UpdateClientDTO
            {
       Id = 1,
       BusinessName = "Test",
  Email = "email-invalido", // Email sin formato correcto
    TaxDocument = "12345678",
        Address = "Test",
       Status = 1
  };

   var result = await clientService.UpdateClientAsync(invalidUpdate);

       Assert.Null(result);
        }

  [Fact(DisplayName = "IT-50d: Actualizar categoría con nombre duplicado debe fallar")]
        public async Task UpdateCategory_WithDuplicateName_ShouldFail()
     {
   using var scope = _factory.Services.CreateScope();
       var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

     var invalidUpdate = new UpdateCategoryDTO
   {
      Id = 1,
   Name = "Alimentos", // Nombre de otra categoría existente
  Description = "Test",
       Status = 1
     };

    var result = await categoryService.UpdateCategoryAsync(invalidUpdate);

            Assert.Null(result);
  }

  #endregion
    }
}
