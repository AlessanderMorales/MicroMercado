using FluentValidation;
using MicroMercado.Application.DTOs.Category;
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
    public class CategoryIntegrationSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private ApplicationDbContext _context;
        private ICategoryService _categoryService;
        private CategoryDTO? _resultCategory;
        private Exception? _exception;
        private IEnumerable<CategoryDTO>? _categoriesList;
        private static byte _nextCategoryId = 1; // Contador estático para IDs

        public CategoryIntegrationSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _nextCategoryId = 1; // Reset para cada escenario
            SetupTestDatabase();
        }

        [When(@"intento actualizar ""(.*)"" con nombre ""(.*)""")]
        public async Task WhenIntentoActualizarConNombre_Alt(string categoriaActual, string nuevoNombre)
        {
            await WhenIntentoActualizarConElNombre(categoriaActual, nuevoNombre);
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
                {
                    options.UseInMemoryDatabase(dbName);
                    options.EnableSensitiveDataLogging(); // Para debugging
                })
                .AddScoped<ICategoryService, CategoryService>()
                .AddScoped<IValidator<CreateCategoryDTO>, MicroMercado.Application.Validators.Category.CreateCategoryValidator>()
                .AddScoped<IValidator<UpdateCategoryDTO>, MicroMercado.Application.Validators.Category.UpdateCategoryValidator>()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning))
                .BuildServiceProvider();

            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            _categoryService = serviceProvider.GetRequiredService<ICategoryService>();
            

            _context.Database.EnsureCreated();
        }

        [When(@"creo una categoria con los siguientes datos:")]
        public async Task WhenCreoUnaCategoria(Table table)
        {
            try
            {
                string name = string.Empty;
                string description = string.Empty;

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
                    }
                }

                var dto = new CreateCategoryDTO
                {
                    Name = name,
                    Description = description
                };

                _resultCategory = await _categoryService.CreateCategoryAsync(dto);
                _scenarioContext["CreationResult"] = _resultCategory;
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["CreationResult"] = null;
            }
        }

        [Then(@"la categoria debe crearse exitosamente")]
        public void ThenLaCategoriDebeCrearseExitosamente()
        {
            Assert.NotNull(_resultCategory);
            Assert.Null(_exception);
        }

        [Then(@"el Status debe ser (.*)")]
        public void ThenElStatusDebeSer(int expectedStatus)
        {
            Assert.Equal((byte)expectedStatus, _resultCategory?.Status);
        }

        [Then(@"el nombre debe ser ""(.*)""")]
        public void ThenElNombreDebeSer(string expectedName)
        {
            Assert.Equal(expectedName, _resultCategory?.Name);
        }

        [Then(@"debe tener un Id generado")]
        public void ThenDebeTenerUnIdGenerado()
        {
            Assert.NotNull(_resultCategory);
            Assert.True(_resultCategory.Id > 0);
        }

        [Given(@"existe una categoria con nombre ""(.*)""")]
        public async Task GivenExisteUnaCategoriaConNombre(string nombre)
        {
            _context.ChangeTracker.Clear();
            
            var category = new Category
            {
                Id = _nextCategoryId++,
                Name = nombre,
                Description = "Descripción de prueba",
                Status = 1,
                LastUpdate = DateTime.Now
            };
            
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            
            Console.WriteLine($"? Created category '{nombre}' with ID: {category.Id}");
            _scenarioContext[$"Category_{nombre}"] = category.Id;
            _scenarioContext["LastCategoryId"] = category.Id;
        }

        [When(@"intento crear una categoria con nombre ""(.*)""")]
        public async Task WhenIntentoCrearUnaCategoriaConNombre(string nombre)
        {
            try
            {
                var dto = new CreateCategoryDTO
                {
                    Name = nombre,
                    Description = "Descripción duplicada"
                };

                _resultCategory = await _categoryService.CreateCategoryAsync(dto);
                _scenarioContext["CreationResult"] = _resultCategory;
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["CreationResult"] = null;
            }
        }

        [Then(@"debe mostrar error de nombre duplicado")]
        public void ThenDebeMostrarErrorDeNombreDuplicado()
        {
            Assert.Null(_resultCategory);
        }

        [When(@"actualizo la categoria con:")]
        public async Task WhenActualizoLaCategoria(Table table)
        {
            try
            {
                _context.ChangeTracker.Clear();
                
                byte categoryId = 0;
                
                if (_scenarioContext.ContainsKey("LastCategoryId"))
                {
                    categoryId = (byte)_scenarioContext["LastCategoryId"];
                }
                else
                {
                    var categoryKey = _scenarioContext.Keys.FirstOrDefault(k => k.StartsWith("Category_"));
                    if (categoryKey != null)
                    {
                        categoryId = (byte)_scenarioContext[categoryKey];
                    }
                }
                
                if (categoryId == 0)
                    throw new InvalidOperationException("No category found in scenario context");
                
                Console.WriteLine($"Retrieved categoryId from context: {categoryId}");
                
                var existingCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == categoryId);
                if (existingCategory == null)
                    throw new InvalidOperationException($"Category with ID {categoryId} not found in database");

                string name = existingCategory.Name;
                string description = existingCategory.Description ?? string.Empty;
                byte status = existingCategory.Status;

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
                        case "Status":
                            status = byte.Parse(valor);
                            break;
                    }
                }

                var dto = new UpdateCategoryDTO
                {
                    Id = categoryId,
                    Name = name,
                    Description = description,
                    Status = status
                };

                Console.WriteLine($"Attempting update - ID: {categoryId}, Name: '{dto.Name}', Description: '{dto.Description}', Status: {dto.Status}");
                
                _resultCategory = await _categoryService.UpdateCategoryAsync(dto);
                _scenarioContext["UpdateResult"] = _resultCategory;
                _scenarioContext["UpdateException"] = null;
                
                if (_resultCategory == null)
                {
                    Console.WriteLine($"? Update returned null!");
                    Console.WriteLine($"  - ID: {categoryId}");
                    Console.WriteLine($"  - Name: '{dto.Name}' (Length: {dto.Name.Length})");
                    Console.WriteLine($"  - Description: '{dto.Description}' (Length: {dto.Description.Length})");
                    Console.WriteLine($"  - Status: {dto.Status}");
                }
                else
                {
                    Console.WriteLine($"? Update successful! New name: '{_resultCategory.Name}'");
                }
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["UpdateResult"] = null;
                _scenarioContext["UpdateException"] = ex;
                Console.WriteLine($"Exception during update: {ex.Message}");
            }
        }

        [Then(@"los datos deben reflejarse en la base de datos")]
        public async Task ThenLosDatosDebenReflejarseEnLaBaseDeDatos()
        {
            Assert.NotNull(_resultCategory);
            _context.ChangeTracker.Clear();
            var dbCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == _resultCategory.Id);
            Assert.NotNull(dbCategory);
            Assert.Equal(_resultCategory.Name, dbCategory.Name);
        }

        [Given(@"existen las siguientes categorias:")]
        public async Task GivenExistenLasSiguientesCategorias(Table table)
        {
            _context.ChangeTracker.Clear();
            
            foreach (var row in table.Rows)
            {
                var category = new Category
                {
                    Id = _nextCategoryId++,
                    Name = row["Name"],
                    Description = row.ContainsKey("Description") ? row["Description"] : "Descripción de prueba",
                    Status = row.ContainsKey("Status") ? byte.Parse(row["Status"]) : (byte)1,
                    LastUpdate = DateTime.Now
                };

                _context.Categories.Add(category);
            }
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        [When(@"intento actualizar ""(.*)"" con el nombre ""(.*)""")]
        public async Task WhenIntentoActualizarConElNombre(string categoriaActual, string nuevoNombre)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoriaActual);
                Assert.NotNull(category);

                var dto = new UpdateCategoryDTO
                {
                    Id = category.Id,
                    Name = nuevoNombre,
                    Description = category.Description,
                    Status = category.Status
                };

                _resultCategory = await _categoryService.UpdateCategoryAsync(dto);
                _scenarioContext["UpdateResult"] = _resultCategory;
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["UpdateResult"] = null;
            }
        }

        [When(@"elimino la categoria")]
        public async Task WhenEliminoLaCategoria()
        {
            var categoryKey = _scenarioContext.Keys.FirstOrDefault(k => k.StartsWith("Category_"));
            if (categoryKey == null)
                throw new InvalidOperationException("No category found in scenario context");
            
            var categoryId = (byte)_scenarioContext[categoryKey];
            await _categoryService.DeleteCategoryAsync(categoryId);

            _context.ChangeTracker.Clear();
            var deletedCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == categoryId);
            _resultCategory = deletedCategory != null ? new CategoryDTO
            {
                Id = deletedCategory.Id,
                Name = deletedCategory.Name,
                Status = deletedCategory.Status
            } : null;
        }

        [Then(@"el Status de la categoria debe cambiar a 0")]
        public void ThenElStatusDeLaCategoriaDebeCambiarA()
        {
            Assert.Equal((byte)0, _resultCategory?.Status);
        }

        [Then(@"la categoria no debe aparecer en busquedas activas")]
        public async Task ThenLaCategoriaNoDebeAparecerEnBusquedasActivas()
        {
            var activeCategories = await _categoryService.GetAllCategoriesAsync();
            Assert.False(activeCategories.Any(c => c.Id == _resultCategory?.Id));
        }

        [Given(@"existen las siguientes categorias activas:")]
        public async Task GivenExistenLasSiguientesCategoriasActivas(Table table)
        {
            _context.ChangeTracker.Clear();
            foreach (var row in table.Rows)
            {
                var category = new Category
                {
                    Id = _nextCategoryId++,
                    Name = row["Name"],
                    Description = row["Description"],
                    Status = 1,
                    LastUpdate = DateTime.Now
                };

                _context.Categories.Add(category);
            }
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        [When(@"consulto todas las categorias")]
        [When(@"obtengo todas las categorias")]
        public async Task WhenConsultoTodasLasCategorias()
        {
            _categoriesList = await _categoryService.GetAllCategoriesAsync();
        }

        [Then(@"debo recibir (.*) categorias")]
        public void ThenDeboRecibirCategorias(int expectedCount)
        {
            Assert.Equal(expectedCount, _categoriesList?.Count());
        }

        [Then(@"todas deben tener Status 1")]
        public void ThenTodasDebenTenerStatus()
        {
            Assert.True(_categoriesList?.All(c => c.Status == 1));
        }

        [AfterScenario]
        public void CleanupDatabase()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}
