using FluentValidation;
using FluentValidation.Results; // Necesario para ValidationResult y ValidationFailure
using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.Services;
using MicroMercado.Domain.Models; // Necesario para las entidades Category
using MicroMercado.Infrastructure.Data; // Necesario para ApplicationDbContext
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit; // Necesario para atributos de Xunit como [Fact], [Theory], etc.

namespace PruebasMicroMercado.WhiteBoxTests
{
    // Nombre de la clase de pruebas siguiendo la convención de Xunit
    public class CategoryServiceTests
    {
        // Helper para obtener un contexto de base de datos en memoria único por cada prueba
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        // Helper para obtener un mock del logger
        private Mock<ILogger<CategoryService>> GetMockLogger()
        {
            return new Mock<ILogger<CategoryService>>();
        }

        // Helper para obtener mocks de los validadores. Por defecto, simulan una validación exitosa.
        private (Mock<IValidator<CreateCategoryDTO>> createValidator, Mock<IValidator<UpdateCategoryDTO>> updateValidator) GetMockValidators()
        {
            var createMock = new Mock<IValidator<CreateCategoryDTO>>();
            var updateMock = new Mock<IValidator<UpdateCategoryDTO>>();

            createMock
                .Setup(v => v.ValidateAsync(It.IsAny<CreateCategoryDTO>(), default))
                .ReturnsAsync(new ValidationResult());

            updateMock
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateCategoryDTO>(), default))
                .ReturnsAsync(new ValidationResult());

            return (createMock, updateMock);
        }

        // Helper para sembrar datos iniciales en la base de datos en memoria para las pruebas
        private async Task SeedTestData(ApplicationDbContext context)
        {
            var category1 = new Category { Id = (byte)1, Name = "Electrónica", Description = "Artículos electrónicos", Status = (byte)1, LastUpdate = DateTime.Now };
            var category2 = new Category { Id = (byte)2, Name = "Alimentos", Description = "Productos comestibles", Status = (byte)1, LastUpdate = DateTime.Now };
            var category3 = new Category { Id = (byte)3, Name = "Ropa", Description = "Vestimenta", Status = (byte)0, LastUpdate = DateTime.Now }; // Categoría inactiva
            var category4 = new Category { Id = (byte)4, Name = "Hogar", Description = "Artículos para el hogar", Status = (byte)1, LastUpdate = DateTime.Now };

            context.Categories.AddRange(category1, category2, category3, category4);
            await context.SaveChangesAsync();
        }

        #region GetAllCategoriesAsync Tests

        // Test: GetAllCategoriesAsync - ShouldReturnOnlyActiveCategories
        // Propósito: Verifica que el método GetAllCategoriesAsync retorna solo las categorías activas (Status = 1).
        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnOnlyActiveCategories()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.GetAllCategoriesAsync();

            Assert.NotNull(result);
            var categories = result.ToList();
            Assert.Equal(3, categories.Count); // Se esperan 3 categorías activas (1, 2, 4)
            Assert.Contains(categories, c => c.Id == (byte)1 && c.Name == "Electrónica");
            Assert.Contains(categories, c => c.Id == (byte)2 && c.Name == "Alimentos");
            Assert.Contains(categories, c => c.Id == (byte)4 && c.Name == "Hogar");
            Assert.DoesNotContain(categories, c => c.Id == (byte)3); // La categoría inactiva no debe estar
        }

        // Test: GetAllCategoriesAsync - ShouldReturnEmptyList_WhenNoActiveCategoriesExist
        // Propósito: Verifica que el método GetAllCategoriesAsync retorna una lista vacía cuando no hay categorías activas.
        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnEmptyList_WhenNoActiveCategoriesExist()
        {
            var context = GetInMemoryDbContext();
            // Sembrar solo una categoría inactiva para asegurar que la lista activa esté vacía
            context.Categories.Add(new Category { Id = (byte)5, Name = "Inactiva", Status = (byte)0, LastUpdate = DateTime.Now });
            await context.SaveChangesAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.GetAllCategoriesAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // Test: GetAllCategoriesAsync - ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método GetAllCategoriesAsync propaga una excepción cuando ocurre un error de base de datos.
        [Fact]
        public async Task GetAllCategoriesAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync(); // Forzar la disposición del contexto para simular un error

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.GetAllCategoriesAsync();
            });
        }

        #endregion

        #region GetCategoryByIdAsync Tests

        // Test: GetCategoryByIdAsync - ShouldReturnCategory_WhenCategoryExistsAndIsActive
        // Propósito: Verifica que el método GetCategoryByIdAsync retorna una categoría activa por su ID.
        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExistsAndIsActive()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.GetCategoryByIdAsync((byte)1); // Electrónica

            Assert.NotNull(result);
            Assert.Equal((byte)1, result.Id);
            Assert.Equal("Electrónica", result.Name);
            Assert.Equal((byte)1, result.Status);
        }

        // Test: GetCategoryByIdAsync - ShouldReturnNull_WhenCategoryDoesNotExist
        // Propósito: Verifica que el método GetCategoryByIdAsync retorna null cuando la categoría no existe.
        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.GetCategoryByIdAsync((byte)99); // ID inexistente

            Assert.Null(result);
        }

        // Test: GetCategoryByIdAsync - ShouldReturnCategory_WhenCategoryExistsButIsInactive
        // Propósito: Verifica que el método GetCategoryByIdAsync retorna una categoría incluso si está inactiva.
        //            (A diferencia de GetAllCategoriesAsync, FindAsync no filtra por Status, solo busca por ID).
        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExistsButIsInactive()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context); // Categoría 3 (Ropa) está inactiva
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.GetCategoryByIdAsync((byte)3); // Ropa (Inactiva)

            Assert.NotNull(result);
            Assert.Equal((byte)3, result.Id);
            Assert.Equal("Ropa", result.Name);
            Assert.Equal((byte)0, result.Status); // Verifica que el status es 0
        }

        // Test: GetCategoryByIdAsync - ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método GetCategoryByIdAsync propaga una excepción cuando ocurre un error de base de datos.
        [Fact]
        public async Task GetCategoryByIdAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync(); // Forzar la disposición para simular un error

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.GetCategoryByIdAsync((byte)1);
            });
        }

        #endregion

        #region CreateCategoryAsync Tests


        // Test: CreateCategoryAsync - ShouldReturnNull_WhenValidationFails
        // Propósito: Verifica que la creación de una categoría falla y retorna null cuando el DTO no pasa la validación.
        [Fact]
        public async Task CreateCategoryAsync_ShouldReturnNull_WhenValidationFails()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();

            // Configurar el mock para que la validación falle
            validators.createValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateCategoryDTO>(), default))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Name", "Name is required") }));

            var createDto = new CreateCategoryDTO { Name = "", Description = "Descripción inválida" }; // Nombre vacío para forzar fallo de validación
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.CreateCategoryAsync(createDto);

            Assert.Null(result);
            // Verificar que el número de categorías en la DB no ha cambiado
            Assert.Equal(4, await context.Categories.CountAsync());

            // Verificar que se emitió un warning de logger por errores de validación
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validation errors creating category")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Test: CreateCategoryAsync - ShouldReturnNull_WhenNameAlreadyExists
        // Propósito: Verifica que la creación de una categoría falla y retorna null cuando ya existe una categoría con el mismo nombre (insensible a mayúsculas/minúsculas).
        [Fact]
        public async Task CreateCategoryAsync_ShouldReturnNull_WhenNameAlreadyExists()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context); // "Electrónica" ya existe
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var createDto = new CreateCategoryDTO { Name = "Electrónica", Description = "Otro nombre igual" }; // Nombre duplicado
            var result = await service.CreateCategoryAsync(createDto);

            Assert.Null(result);
            // Verificar que el número de categorías en la DB no ha cambiado
            Assert.Equal(4, await context.Categories.CountAsync());

            // Verificar que se emitió un warning de logger por nombre duplicado
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Category with name Electrónica already exists.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Test: CreateCategoryAsync - ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método CreateCategoryAsync propaga una excepción (y la registra)
        //            cuando ocurre un error inesperado en la base de datos durante el proceso de guardado.
        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync(); // Forzar la disposición para simular un error de DB

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var createDto = new CreateCategoryDTO { Name = "Muebles", Description = "Muebles para el hogar" };
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.CreateCategoryAsync(createDto);
            });
        }

        #endregion

        #region UpdateCategoryAsync Tests

        // Test: UpdateCategoryAsync - ShouldUpdateCategory_WhenDataIsValidAndUnique
        // Propósito: Verifica que una categoría existente se actualiza exitosamente cuando el DTO es válido y el nombre actualizado es único.
        [Fact]
        public async Task UpdateCategoryAsync_ShouldUpdateCategory_WhenDataIsValidAndUnique()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators(); // Configurado para validación exitosa por defecto
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var updateDto = new UpdateCategoryDTO { Id = (byte)1, Name = "Electrónica Actualizada", Description = "Nueva descripción", Status = (byte)1 };
            var result = await service.UpdateCategoryAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal((byte)1, result.Id);
            Assert.Equal("Electrónica Actualizada", result.Name);
            Assert.Equal("Nueva descripción", result.Description);
            Assert.Equal((byte)1, result.Status); // Verificar que el Status se mantuvo

            // Verificar que la categoría realmente se actualizó en la base de datos
            var categoryInDb = await context.Categories.FindAsync((byte)1);
            Assert.NotNull(categoryInDb);
            Assert.Equal("Electrónica Actualizada", categoryInDb.Name);
            Assert.Equal("Nueva descripción", categoryInDb.Description);
            Assert.Equal((byte)1, categoryInDb.Status);
            Assert.True(categoryInDb.LastUpdate > DateTime.Now.AddMinutes(-1)); // LastUpdate debería haberse actualizado

            // Verificar que no se emitió ningún warning de logger
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        // Test: UpdateCategoryAsync - ShouldReturnNull_WhenValidationFails
        // Propósito: Verifica que la actualización de una categoría falla y retorna null cuando el DTO no pasa la validación.
        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnNull_WhenValidationFails()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();

            // Configurar el mock para que la validación falle
            validators.updateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateCategoryDTO>(), default))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Name", "Name is required") }));

            var updateDto = new UpdateCategoryDTO { Id = (byte)1, Name = "", Description = "Descripción inválida", Status = (byte)1 }; // Nombre vacío
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.UpdateCategoryAsync(updateDto);

            Assert.Null(result);
            // Verificar que la categoría original en la DB no se modificó
            var originalCategory = await context.Categories.FindAsync((byte)1);
            Assert.Equal("Electrónica", originalCategory.Name);

            // Verificar que se emitió un warning de logger por errores de validación
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validation errors updating category")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Test: UpdateCategoryAsync - ShouldReturnNull_WhenCategoryDoesNotExist
        // Propósito: Verifica que la actualización de una categoría falla y retorna null cuando la categoría a actualizar no existe.
        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var updateDto = new UpdateCategoryDTO { Id = (byte)99, Name = "Categoría Inexistente", Description = "Descripción", Status = (byte)1 };
            var result = await service.UpdateCategoryAsync(updateDto);

            Assert.Null(result);

            // Verificar que se emitió un warning de logger por categoría no encontrada
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Category with ID 99 not found for update.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Test: UpdateCategoryAsync - ShouldReturnNull_WhenNameAlreadyExistsOnAnotherCategory
        // Propósito: Verifica que la actualización de una categoría falla y retorna null
        //            cuando el nuevo nombre ya está en uso por otra categoría existente.
        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnNull_WhenNameAlreadyExistsOnAnotherCategory()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context); // Categoría 1: "Electrónica", Categoría 2: "Alimentos"
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var updateDto = new UpdateCategoryDTO { Id = (byte)1, Name = "Alimentos", Description = "Intentando duplicar", Status = (byte)1 }; // Cambiar Electrónica a "Alimentos"
            var result = await service.UpdateCategoryAsync(updateDto);

            Assert.Null(result);
            // Verificar que la categoría original en la DB no se modificó
            var originalCategory = await context.Categories.FindAsync((byte)1);
            Assert.Equal("Electrónica", originalCategory.Name);

            // Verificar que se emitió un warning de logger por nombre duplicado
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Another category with name Alimentos already exists.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Test: UpdateCategoryAsync - ShouldSucceed_WhenNameIsSameAsOwnCategory
        // Propósito: Verifica que la actualización de una categoría es exitosa incluso si el nombre
        //            no cambia (es decir, el mismo nombre es del propio producto que se está actualizando),
        //            solo se actualizan otras propiedades.
        [Fact]
        public async Task UpdateCategoryAsync_ShouldSucceed_WhenNameIsSameAsOwnCategory()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context); // Categoría 1: "Electrónica"
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var updateDto = new UpdateCategoryDTO { Id = (byte)1, Name = "Electrónica", Description = "Descripción actualizada", Status = (byte)1 };
            var result = await service.UpdateCategoryAsync(updateDto);

            Assert.NotNull(result);
            Assert.Equal((byte)1, result.Id);
            Assert.Equal("Electrónica", result.Name);
            Assert.Equal("Descripción actualizada", result.Description);
        }

        // Test: UpdateCategoryAsync - ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método UpdateCategoryAsync propaga una excepción (y la registra)
        //            cuando ocurre un error inesperado en la base de datos durante la actualización.
        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync(); // Forzar la disposición para simular un error de DB

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var updateDto = new UpdateCategoryDTO { Id = (byte)1, Name = "Nombre", Description = "Desc", Status = (byte)1 };
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.UpdateCategoryAsync(updateDto);
            });
        }

        #endregion

        #region DeleteCategoryAsync Tests (Borrado Físico / Hard Delete)

        // Test: DeleteCategoryAsync - ShouldReturnTrue_AndPerformPhysicalDelete_WhenCategoryExists
        // Propósito: Verifica que el borrado físico de una categoría es exitoso para una categoría existente
        //            (eliminándola permanentemente de la BD) y que retorna true. También verifica el log de información.
        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnTrue_AndPerformLogicalDelete_WhenCategoryExists()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context); // Categoría 1: Electrónica (Status=1)
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.DeleteCategoryAsync((byte)1);

            Assert.True(result);

            // ✅ HARD DELETE: La categoría ya NO debe existir en la BD
            var categoryInDb = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == (byte)1);
            Assert.Null(categoryInDb); // ← La categoría fue eliminada físicamente

            // Verificar Logger
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Category with ID 1 permanently deleted")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Test: DeleteCategoryAsync - ShouldReturnFalse_WhenCategoryDoesNotExist
        // Propósito: Verifica que el borrado físico de una categoría retorna false
        //            cuando la categoría no existe y registra un warning.
        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            var result = await service.DeleteCategoryAsync((byte)99); // ID inexistente

            Assert.False(result);

            // Verificar Logger
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt to delete category ID 99 but not found.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Test: DeleteCategoryAsync - ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método DeleteCategoryAsync propaga una excepción (y la registra)
        //            cuando ocurre un error inesperado en la base de datos durante el borrado.
        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync(); // Forzar la disposición para simular un error de DB

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new CategoryService(context, validators.createValidator.Object, validators.updateValidator.Object, logger.Object);

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.DeleteCategoryAsync((byte)1);
            });
        }

        #endregion
    }
}