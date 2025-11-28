using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Application.Services;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Logging;
using Xunit;
using Reqnroll;

namespace Pruebas_BBD.StepDefinitions
{
    [Binding]
    public class SalesIntegrationSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private ApplicationDbContext _context;
        private ISaleService _saleService;
        private SaleDTO.CreateSaleDTO _saleDto;
        private SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>? _saleResult;
        private readonly Dictionary<string, short> _productIds = new();

        public SalesIntegrationSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            SetupTestDatabase();
        }

        private void SetupTestDatabase()
        {
            const string key = "InMemoryDbName";
            if (!_scenarioContext.ContainsKey(key))
            {
                _scenarioContext[key] = $"MicroMercado_TestDB_{Guid.NewGuid()}";
            }
            var dbName = _scenarioContext[key].ToString();

            var serviceProvider = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(dbName)
                           .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)))
                .AddScoped<ISaleService, SaleService>()
                .AddScoped<IProductService, ProductService>()
                .AddScoped(typeof(FluentValidation.IValidator<MicroMercado.Application.DTOs.Product.CreateProductDTO>), typeof(MicroMercado.Application.Validators.Product.CreateProductValidator))
                .AddScoped(typeof(FluentValidation.IValidator<MicroMercado.Application.DTOs.Product.UpdateProductDTO>), typeof(MicroMercado.Application.Validators.Product.UpdateProductValidator))
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            _saleService = serviceProvider.GetRequiredService<ISaleService>();

            _saleDto = new SaleDTO.CreateSaleDTO
            {
                Items = new List<SaleDTO.SaleItemDTO>()
            };
        }

        [Given(@"existen los siguientes productos:")]
        public async Task GivenExistenLosSiguientesProductos(Table table)
        {
            foreach (var row in table.Rows)
            {
                var product = new Product
                {
                    Name = row["Name"],
                    Description = "Descripción de prueba",
                    Brand = "Marca Test",
                    Price = decimal.Parse(row["Price"]),
                    Stock = short.Parse(row["Stock"]),
                    CategoryId = byte.Parse(row["CategoryId"]),
                    Status = 1,
                    LastUpdate = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                
                _productIds[row["Name"]] = product.Id;
                _scenarioContext[$"Product_{row["Name"]}"] = product.Id;
            }
        }

        [Given(@"existe un cliente ""(.*)"" con TaxDocument ""(.*)"" para ventas")]
        public async Task GivenExisteUnClienteParaVentas(string nombre, string taxDocument)
        {
            var client = new Client
            {
                BusinessName = nombre,
                Email = $"{nombre.Replace(" ", "").ToLower()}@example.com",
                TaxDocument = taxDocument,
                Address = "Dirección de prueba",
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _scenarioContext["ClientId"] = client.Id;
        }

        [Given(@"tengo los siguientes productos en el carrito:")]
        public void GivenTengoLosSiguientesProductosEnElCarrito(Table table)
        {
            _saleDto.Items = new List<SaleDTO.SaleItemDTO>();

            foreach (var row in table.Rows)
            {
                var productName = row["ProductName"];
                var quantity = short.Parse(row["Quantity"]);

                if (_productIds.TryGetValue(productName, out var productId))
                {
                    var product = _context.Products.Find(productId);
                    
                    _saleDto.Items.Add(new SaleDTO.SaleItemDTO
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product!.Price
                    });
                }
            }
        }

        [When(@"proceso la venta con los siguientes datos:")]
        public async Task WhenProcesoLaVentaConLosSiguientesDatos(Table table)
        {
            _saleDto.ClientId = (int)_scenarioContext["ClientId"];
            foreach (var row in table.Rows)
            {
                var campo = row["Campo"].Trim();
                var valor = row["Valor"].Trim();
                if (campo.Equals("ClientId", StringComparison.OrdinalIgnoreCase))
                {
                    _saleDto.ClientId = int.Parse(valor);
                }
                else if (campo.Equals("PaymentType", StringComparison.OrdinalIgnoreCase))
                {
                    _saleDto.PaymentType = valor.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) ? (byte)1 : (byte)2;
                }
                else if (campo.Equals("CashReceived", StringComparison.OrdinalIgnoreCase))
                {
                    _saleDto.CashReceived = decimal.Parse(valor);
                }
            }

            _saleDto.TotalAmount = _saleDto.Items.Sum(i => i.Quantity * i.Price);
            _saleDto.Change = _saleDto.CashReceived - _saleDto.TotalAmount;
            _saleResult = await _saleService.CreateSaleAsync(_saleDto);
        }

        [Then(@"la venta debe registrarse exitosamente")]
        public void ThenLaVentaDebeRegistrarseExitosamente()
        {
            Assert.NotNull(_saleResult);
            Assert.True(_saleResult.Success);
        }

        [Then(@"el total debe ser (.*)")]
        public void ThenElTotalDebeSer(decimal expectedTotal)
        {
            Assert.Equal(expectedTotal, _saleDto.TotalAmount, 2);
        }

        [Then(@"el cambio debe ser (.*)")]
        public void ThenElCambioDebeSer(decimal expectedChange)
        {
            Assert.Equal(expectedChange, _saleDto.Change, 2);
        }

        [Then(@"el stock de ""(.*)"" debe ser (.*)")]
        public async Task ThenElStockDeDebeSer(string productName, short expectedStock)
        {
            if (_productIds.TryGetValue(productName, out var productId))
            {
                _context.ChangeTracker.Clear();
                var product = await _context.Products.FindAsync(productId);
                
                Assert.NotNull(product);
                Assert.Equal(expectedStock, product.Stock);
            }
            else
            {
                throw new Exception($"No se encontró el producto '{productName}' en el diccionario");
            }
        }

        [Then(@"debe existir (.*) registro en Sales")]
        public void ThenDebeExistirRegistroEnSales(int expectedCount)
        {
            var salesCount = _context.Sales.Count();
            Assert.Equal(expectedCount, salesCount);
        }

        [Then(@"deben existir (.*) registros en SaleItems")]
        public void ThenDebenExistirRegistrosEnSaleItems(int expectedCount)
        {
            var saleItemsCount = _context.SaleItems.Count();
            Assert.Equal(expectedCount, saleItemsCount);
        }

        [Then(@"la fecha de venta debe ser la actual")]
        public void ThenLaFechaDeVentaDebeSerLaActual()
        {
            var sale = _context.Sales.FirstOrDefault();
            Assert.NotNull(sale);
            
            var differenceInMinutes = Math.Abs((DateTime.Now - sale.SaleDate).TotalMinutes);
            Assert.True(differenceInMinutes < 5);
        }

        [AfterScenario]
        public void CleanupDatabase()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}
