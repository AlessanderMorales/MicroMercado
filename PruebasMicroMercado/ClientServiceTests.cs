using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MicroMercado.Data;
using MicroMercado.DTOs;
using MicroMercado.Models;
using MicroMercado.Services;
using Xunit;

namespace PruebasMicroMercado
{
    public class ClientServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetClientByIdAsync_ReturnsClient_WhenExists()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);
            var client = new Client { Name = "Juan", LastName = "Perez", TaxDocument = "1234567" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var result = await service.GetClientByIdAsync(client.Id);

            Assert.NotNull(result);
            Assert.Equal("Juan", result!.Name);
        }

        [Fact]
        public async Task GetClientByTaxDocumentAsync_ReturnsClient_WhenExists()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);
            var client = new Client { Name = "Ana", LastName = "Gomez", TaxDocument = "7654321" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var result = await service.GetClientByTaxDocumentAsync("7654321");

            Assert.NotNull(result);
            Assert.Equal("Ana", result!.Name);
        }

        [Fact]
        public async Task GetAllClientsAsync_ReturnsAllClients()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);
            context.Clients.Add(new Client { Name = "A", LastName = "One", TaxDocument = "1" });
            context.Clients.Add(new Client { Name = "B", LastName = "Two", TaxDocument = "2" });
            await context.SaveChangesAsync();

            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var all = await service.GetAllClientsAsync();

            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task CreateClientAsync_ReturnsNull_WhenValidationFails()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);

            var invalidResult = new ValidationResult(new[] { new ValidationFailure("Name", "Required") });
            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            createValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateClientDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidResult);

            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var dto = new CreateClientDTO { Name = "", LastName = "", TaxDocument = "" };
            var result = await service.CreateClientAsync(dto);

            Assert.Null(result);
            Assert.Empty(context.Clients);
        }

        [Fact]
        public async Task CreateClientAsync_CreatesClient_WhenValidationPasses()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);

            var validResult = new ValidationResult();
            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            createValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateClientDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validResult);

            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var dto = new CreateClientDTO { Name = "Carlos", LastName = "Lopez", TaxDocument = "9999999" };
            var result = await service.CreateClientAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(1, context.Clients.Count());
            Assert.Equal("Carlos", result!.Name);
        }

        [Fact]
        public async Task UpdateClientAsync_ReturnsNull_WhenValidationFails()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);
            var client = new Client { Name = "Old", LastName = "Name", TaxDocument = "111" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var invalidResult = new ValidationResult(new[] { new ValidationFailure("Name", "Invalid") });
            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            updateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidResult);

            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var dto = new UpdateClientDTO { Id = client.Id, Name = "New", LastName = "NewLast", TaxDocument = "222", Status = 1 };
            var result = await service.UpdateClientAsync(dto);

            Assert.Null(result);
            var still = await context.Clients.FindAsync(client.Id);
            Assert.Equal("Old", still!.Name);
        }

        [Fact]
        public async Task UpdateClientAsync_ReturnsNull_WhenClientNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);

            var validResult = new ValidationResult();
            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            updateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validResult);

            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var dto = new UpdateClientDTO { Id = 999, Name = "X", LastName = "Y", TaxDocument = "333", Status = 1 };
            var result = await service.UpdateClientAsync(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateClientAsync_UpdatesClient_WhenValid()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);
            var client = new Client { Name = "Antes", LastName = "Antes", TaxDocument = "444" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var validResult = new ValidationResult();
            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            updateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validResult);

            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var dto = new UpdateClientDTO { Id = client.Id, Name = "Despues", LastName = "Despues", TaxDocument = "555", Status = 1 };
            var result = await service.UpdateClientAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Despues", result!.Name);
            var fromDb = await context.Clients.FindAsync(client.Id);
            Assert.Equal("Despues", fromDb!.Name);
        }

        [Fact]
        public async Task DeleteClientAsync_ReturnsFalse_WhenClientNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);

            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var result = await service.DeleteClientAsync(12345);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteClientAsync_DeletesClient_WhenExists()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateInMemoryContext(dbName);
            var client = new Client { Name = "ToDelete", LastName = "X", TaxDocument = "666" };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var createValidator = new Mock<IValidator<CreateClientDTO>>();
            var updateValidator = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidator.Object, updateValidator.Object);

            var result = await service.DeleteClientAsync(client.Id);

            Assert.True(result);
            Assert.Empty(context.Clients);
        }
    }
}