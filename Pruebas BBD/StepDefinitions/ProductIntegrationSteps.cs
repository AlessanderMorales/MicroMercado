using FluentValidation;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Reqnroll;

namespace Pruebas_BBD.StepDefinitions
{
    [Binding]
    public class ProductIntegrationSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private ApplicationDbContext _context;
        private IProductService _productService;
        private Product? _resultProduct;
        private Exception? _exception;
        private IEnumerable<ProductDTO>? _productsList;
        private static short _nextProductId = 1;
        private static byte _nextCategoryId = 1;

        public ProductIntegrationSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _nextProductId = 1;
            _nextCategoryId = 1;
            SetupTestDatabase();
        }

        private void SetupTestDatabase()
        {
            const string dbKey = "InMemoryDbName";
            if (!_scenarioContext.ContainsKey(dbKey))
            {
                _scenarioContext[dbKey] = $"MicroMercado_TestDB_{Guid.NewGuid()}";
            }

            var dbName = _scenarioContext[dbKey].ToString();

            var serviceProvider = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(dbName))
                .AddScoped<IProductService, ProductService>()
                .AddScoped<IValidator<CreateProductDTO>, MicroMercado.Application.Validators.Product.CreateProductValidator>()
                .AddScoped<IValidator<UpdateProductDTO>, MicroMercado.Application.Validators.Product.UpdateProductValidator>()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            _productService = serviceProvider.GetRequiredService<IProductService>();
        }

        [Given(@"existe una categoria ""(.*)"" con id (.*)")]
        public async Task GivenExisteUnaCategoria(string nombre, byte id)
        {
            _context.ChangeTracker.Clear();
            
            var category = new Category
            {
                Id = id,
                Name = nombre,
                Description = $"Descripción de {nombre}",
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _scenarioContext["CategoryId"] = id;
            _nextCategoryId = (byte)(id + 1);
        }

        [When(@"creo un producto con los siguientes datos:")]
        public async Task WhenCreoUnProducto(Table table)
        {
            try
            {
                string name = string.Empty;
                string description = string.Empty;
                string brand = string.Empty;
                decimal price = 0;
                short stock = 0;
                byte categoryId = 0;

                foreach (var row in table.Rows)
                {
                    var campo = row["Campo"];
                    var valor = row["Valor"];

                    switch (campo)
                    {
                        case "Name":
                            name = valor;
                            break;
                        case "Description":
                            description = valor;
                            break;
                        case "Brand":
                            brand = valor;
                            break;
                        case "Price":
                            price = decimal.Parse(valor);
                            break;
                        case "Stock":
                            stock = short.Parse(valor);
                            break;
                        case "CategoryId":
                            categoryId = byte.Parse(valor);
                            break;
                    }
                }

                var dto = new CreateProductDTO
                {
                    Name = name,
                    Description = description,
                    Brand = brand,
                    Price = price,
                    Stock = stock,
                    CategoryId = categoryId
                };

                _resultProduct = await _productService.CreateProductAsync(dto);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then(@"el producto debe crearse exitosamente")]
        public void ThenElProductoDebeCrearseExitosamente()
        {
            Assert.NotNull(_resultProduct);
            Assert.Null(_exception);
            _scenarioContext["CreationResult"] = _resultProduct;
        }

        [Then(@"debe tener Stock (.*)")]
        public void ThenDebeTenerStock(short expectedStock)
        {
            Assert.Equal(expectedStock, _resultProduct?.Stock);
        }

        [Then(@"debe estar asociado a la categoria (.*)")]
        public void ThenDebeEstarAsociadoALaCategoria(byte categoryId)
        {
            Assert.Equal(categoryId, _resultProduct?.CategoryId);
        }

        [When(@"intento crear un producto con precio (.*)")]
        public async Task WhenIntentoCrearUnProductoConPrecio(decimal precio)
        {
            try
            {
                var dto = new CreateProductDTO
                {
                    Name = "Producto Test",
                    Description = "Descripción",
                    Brand = "Marca",
                    Price = precio,
                    Stock = 10,
                    CategoryId = 1
                };

                _resultProduct = await _productService.CreateProductAsync(dto);
                _scenarioContext["CreationResult"] = _resultProduct;
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["CreationResult"] = null;
            }
        }

        [Then(@"debe mostrar error de validacion de precio")]
        public void ThenDebeMostrarErrorDeValidacionDePrecio()
        {
            Assert.Null(_resultProduct);
        }

        [Given(@"existe un producto ""(.*)"" con precio (.*) y stock (.*)")]
        public async Task GivenExisteUnProducto(string nombre, decimal precio, short stock)
        {
            _context.ChangeTracker.Clear();
            
            var product = new Product
            {
                Id = _nextProductId++,
                Name = nombre,
                Description = "Descripción de prueba",
                Brand = "Marca Test",
                Price = precio,
                Stock = stock,
                CategoryId = 1,
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _scenarioContext["ProductId"] = product.Id;
        }

        [When(@"actualizo el producto con:")]
        public async Task WhenActualizoElProductoCon(Table table)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var productId = (short)_scenarioContext["ProductId"];
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
                
                if (existingProduct == null)
                    throw new InvalidOperationException($"Product with ID {productId} not found");

                string name = existingProduct.Name;
                string description = existingProduct.Description;
                string brand = existingProduct.Brand;
                decimal price = existingProduct.Price;
                short stock = existingProduct.Stock;
                byte categoryId = existingProduct.CategoryId;
                byte status = existingProduct.Status;

                foreach (var row in table.Rows)
                {
                    var campo = row["Campo"];
                    var valor = row["Valor"];

                    switch (campo)
                    {
                        case "Name":
                            name = valor;
                            break;
                        case "Description":
                            description = valor;
                            break;
                        case "Brand":
                            brand = valor;
                            break;
                        case "Price":
                            price = decimal.Parse(valor);
                            break;
                        case "Stock":
                            stock = short.Parse(valor);
                            break;
                        case "CategoryId":
                            categoryId = byte.Parse(valor);
                            break;
                        case "Status":
                            status = byte.Parse(valor);
                            break;
                    }
                }

                var dto = new UpdateProductDTO
                {
                    Id = productId,
                    Name = name,
                    Description = description,
                    Brand = brand,
                    Price = price,
                    Stock = stock,
                    CategoryId = categoryId,
                    Status = status
                };

                _resultProduct = await _productService.UpdateProductAsync(dto);
                _scenarioContext["UpdateResult"] = _resultProduct;
                _scenarioContext["UpdateException"] = null;
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["UpdateResult"] = null;
                _scenarioContext["UpdateException"] = ex;
            }
        }

        [Then(@"el precio debe ser (.*)")]
        public void ThenElPrecioDebeSer(decimal expectedPrice)
        {
            Assert.Equal(expectedPrice, _resultProduct?.Price);
        }

        [Then(@"el stock debe ser (.*)")]
        public void ThenElStockDebeSer(short expectedStock)
        {
            Assert.Equal(expectedStock, _resultProduct?.Stock);
        }

        [Then(@"los nuevos datos deben estar guardados")]
        public async Task ThenLosNuevosDatosDebenEstarGuardados()
        {
            var dbProduct = await _context.Products.FindAsync(_resultProduct?.Id);
            Assert.NotNull(dbProduct);
            Assert.Equal(_resultProduct?.Name, dbProduct.Name);
        }

        [Given(@"existe un producto ""(.*)"" con stock (.*)")]
        public async Task GivenExisteUnProductoConStock(string nombre, short stock)
        {
            _context.ChangeTracker.Clear();
            
            var product = new Product
            {
                Id = _nextProductId++,
                Name = nombre,
                Description = "Descripción",
                Brand = "Marca",
                Price = 10.00m,
                Stock = stock,
                CategoryId = 1,
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _scenarioContext["ProductId"] = product.Id;
        }

        [When(@"intento actualizar el stock a (.*)")]
        public async Task WhenIntentoActualizarElStockA(short nuevoStock)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var productId = (short)_scenarioContext["ProductId"];
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);

                var dto = new UpdateProductDTO
                {
                    Id = productId,
                    Name = existingProduct!.Name,
                    Description = existingProduct.Description,
                    Brand = existingProduct.Brand,
                    Price = existingProduct.Price,
                    Stock = nuevoStock,
                    CategoryId = existingProduct.CategoryId,
                    Status = existingProduct.Status
                };

                _resultProduct = await _productService.UpdateProductAsync(dto);
                _scenarioContext["UpdateResult"] = _resultProduct;
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["UpdateResult"] = null;
            }
        }

        [Then(@"debe mostrar error de validacion de stock")]
        public void ThenDebeMostrarErrorDeValidacionDeStock()
        {
            Assert.Null(_resultProduct);
        }

        [Given(@"existe un producto ""(.*)""")]
        public async Task GivenExisteUnProducto(string nombre)
        {
            _context.ChangeTracker.Clear();
            
            var product = new Product
            {
                Id = _nextProductId++,
                Name = nombre,
                Description = "Descripción temporal",
                Brand = "Marca",
                Price = 5.00m,
                Stock = 10,
                CategoryId = 1,
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _scenarioContext["ProductId"] = product.Id;
        }

        [When(@"elimino el producto")]
        public async Task WhenEliminoElProducto()
        {
            var productId = (short)_scenarioContext["ProductId"];
            await _productService.DeleteProductAsync(productId);

            _context.ChangeTracker.Clear();
            _resultProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
        }

        [Then(@"el producto no debe aparecer en busquedas activas")]
        public async Task ThenElProductoNoDebeAparecerEnBusquedasActivas()
        {
            var activeProducts = await _productService.GetAllProductsAsync();
            Assert.False(activeProducts.Any(p => p.Id == _resultProduct?.Id));
        }

        [Given(@"existen los siguientes productos activos en categoria (.*):")
]
        public async Task GivenExistenLosSiguientesProductosActivos(int categoryId, Table table)
        {
            _context.ChangeTracker.Clear();
            
            foreach (var row in table.Rows)
            {
                var product = new Product
                {
                    Id = _nextProductId++,
                    Name = row["Name"],
                    Description = "Descripción",
                    Brand = "Marca",
                    Price = decimal.Parse(row["Price"]),
                    Stock = short.Parse(row["Stock"]),
                    CategoryId = (byte)categoryId,
                    Status = 1,
                    LastUpdate = DateTime.Now
                };

                _context.Products.Add(product);
            }
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        [When(@"busco productos de la categoria (.*)")]
        public async Task WhenBuscoProductosDeLaCategoria(byte categoryId)
        {
            _productsList = await _productService.GetAllProductsAsync();
            _productsList = _productsList.Where(p => p.CategoryId == categoryId);
        }

        [Then(@"debo recibir (.*) productos")]
        public void ThenDeboRecibirProductos(int expectedCount)
        {
            Assert.Equal(expectedCount, _productsList?.Count());
        }

        [Then(@"todos deben tener Status 1")]
        public void ThenTodosDebenTenerStatus1()
        {
            Assert.True(_productsList?.All(p => p.Status == 1));
        }

        [Then(@"el Status del producto debe cambiar a 0")]
        public void ThenElStatusDelProductoDebeCambiarA()
        {
            Assert.NotNull(_resultProduct);
            Assert.Equal((byte)0, _resultProduct.Status);
        }

        [AfterScenario]
        public void CleanupDatabase()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}

