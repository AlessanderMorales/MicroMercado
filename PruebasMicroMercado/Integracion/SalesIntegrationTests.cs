using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Application.Services;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.Integracion
{
    [Collection("IntegrationTests")]
    public class SalesIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

   public SalesIntegrationTests(CustomWebApplicationFactory<Program> factory)
      {
    _factory = factory;
        }

        #region IT-26: Creación de venta completa con múltiples productos

        [Fact(DisplayName = "IT-26: Creación de venta completa debe crear venta, items y actualizar stock")]
        public async Task CreateSale_WithMultipleProducts_ShouldCreateSaleItemsAndUpdateStock()
    {
 using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Arrange - Obtener stock inicial
            var product1InitialStock = await context.Products.Where(p => p.Id == 1).Select(p => p.Stock).FirstAsync();
     var product2InitialStock = await context.Products.Where(p => p.Id == 2).Select(p => p.Stock).FirstAsync();

       var createSaleDTO = new SaleDTO.CreateSaleDTO
    {
       ClientId = 1,
      TotalAmount = 36.50m,
     CashReceived = 40.00m,
      Change = 3.50m,
     PaymentType = 1, // Cambio de "Efectivo" a 1 (byte)
        Items = new List<SaleDTO.SaleItemDTO>
    {
         new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 2, Price = 10.00m, Total = 20.00m },
    new SaleDTO.SaleItemDTO { ProductId = 2, Quantity = 2, Price = 8.25m, Total = 16.50m }
     }
    };

      // Act
      var result = await saleService.CreateSaleAsync(createSaleDTO);

    // Assert
     Assert.True(result.Success);
    Assert.NotNull(result.Data);
     Assert.Equal(36.50m, result.Data.TotalAmount);
  Assert.Equal(2, result.Data.ItemsCount);

    // Verificar que la venta se creó en la BD
            var sale = await context.Sales
 .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.Id == result.Data.SaleId);

            Assert.NotNull(sale);
    Assert.Equal(2, sale.SaleItems.Count);
    Assert.Equal(36.50m, sale.TotalAmount);

            // Verificar que el stock se actualizó correctamente
            var product1FinalStock = await context.Products.Where(p => p.Id == 1).Select(p => p.Stock).FirstAsync();
   var product2FinalStock = await context.Products.Where(p => p.Id == 2).Select(p => p.Stock).FirstAsync();

            Assert.Equal(product1InitialStock - 2, product1FinalStock);
          Assert.Equal(product2InitialStock - 2, product2FinalStock);
        }

        #endregion

      #region IT-27: Venta con cliente inexistente debe fallar

   [Fact(DisplayName = "IT-27: Venta con cliente inexistente debe fallar")]
        public async Task CreateSale_WithNonExistentClient_ShouldFail()
        {
using var scope = _factory.Services.CreateScope();
  var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();

  var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
         ClientId = 999, // Cliente inexistente
                TotalAmount = 20.00m,
          CashReceived = 20.00m,
       Change = 0m,
                Items = new List<SaleDTO.SaleItemDTO>
    {
       new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1, Price = 10.00m, Total = 10.00m }
       }
     };

    var result = await saleService.CreateSaleAsync(createSaleDTO);

            Assert.False(result.Success);
  Assert.Contains("no encontrado", result.Message.ToLower());
        }

        #endregion

        #region IT-28: Venta con stock insuficiente debe fallar y revertir transacción

  [Fact(DisplayName = "IT-28: Venta con stock insuficiente debe fallar y revertir transacción")]
        public async Task CreateSale_WithInsufficientStock_ShouldFailAndRollback()
  {
            using var scope = _factory.Services.CreateScope();
     var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

     var initialSalesCount = await context.Sales.CountAsync();
    var product1InitialStock = await context.Products.Where(p => p.Id == 1).Select(p => p.Stock).FirstAsync();

            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
            ClientId = null,
            TotalAmount = 1000.00m,
          CashReceived = 1000.00m,
       Change = 0m,
         Items = new List<SaleDTO.SaleItemDTO>
          {
           new SaleDTO.SaleItemDTO { ProductId = 1, Quantity = 1000, Price = 10.00m, Total = 10000.00m } // Cantidad mayor al stock
    }
            };

  var result = await saleService.CreateSaleAsync(createSaleDTO);

            Assert.False(result.Success);
     Assert.Contains("stock", result.Message.ToLower());

            // Verificar que no se creó ninguna venta
      var finalSalesCount = await context.Sales.CountAsync();
            Assert.Equal(initialSalesCount, finalSalesCount);

            // Verificar que el stock no cambió
    var product1FinalStock = await context.Products.Where(p => p.Id == 1).Select(p => p.Stock).FirstAsync();
     Assert.Equal(product1InitialStock, product1FinalStock);
        }

        #endregion

    #region IT-29: Venta sin productos debe fallar

        [Fact(DisplayName = "IT-29: Venta sin productos debe fallar")]
  public async Task CreateSale_WithNoItems_ShouldFail()
        {
      using var scope = _factory.Services.CreateScope();
      var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createSaleDTO = new SaleDTO.CreateSaleDTO
            {
       ClientId = null,
    TotalAmount = 0m,
        CashReceived = 0m,
       Change = 0m,
           Items = new List<SaleDTO.SaleItemDTO>() // Sin items
          };

            var result = await saleService.CreateSaleAsync(createSaleDTO);

      Assert.False(result.Success);
            Assert.Equal("No hay productos en la venta", result.Message);
        }

        #endregion

        #region IT-30: Múltiples ventas concurrentes (simulación)

        [Fact(DisplayName = "IT-30: Múltiples ventas secuenciales deben actualizar stock correctamente")]
        public async Task CreateMultipleSales_Sequentially_ShouldUpdateStockCorrectly()
        {
         using var scope = _factory.Services.CreateScope();
            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
       var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

  var initialStock = await context.Products.Where(p => p.Id == 1).Select(p => p.Stock).FirstAsync();

    // Crear 3 ventas secuenciales del mismo producto
   for (int i = 0; i < 3; i++)
            {
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

      var result = await saleService.CreateSaleAsync(createSaleDTO);
   Assert.True(result.Success);
        }

     // Verificar que el stock se redujo correctamente
            var finalStock = await context.Products.Where(p => p.Id == 1).Select(p => p.Stock).FirstAsync();
            Assert.Equal(initialStock - 3, finalStock);
        }

      #endregion
    }
}
