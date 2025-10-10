using FluentValidation;
using FluentValidation.Results;
using MicroMercado.Application.Services;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado
{
    public class ClientServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        // Test 1: GetClientByIdAsync - Complexity 1
        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenClientExists()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);

            var client = new Client
            {
                Id = 1,
                Name = "Juan",
                LastName = "Pérez",
                TaxDocument = "12345678",
                Status = 1,
                LastUpdate = DateTime.Now
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var result = await service.GetClientByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Juan", result.Name);
            Assert.Equal("Pérez", result.LastName);
        }

        // Test 2: GetClientByTaxDocumentAsync - Complexity 1
        [Fact]
        public async Task GetClientByTaxDocumentAsync_ShouldReturnClient_WhenTaxDocumentExists()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);

            var client = new Client
            {
                Id = 1,
                Name = "María",
                LastName = "González",
                TaxDocument = "87654321",
                Status = 1,
                LastUpdate = DateTime.Now
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            var result = await service.GetClientByTaxDocumentAsync("87654321");
            Assert.NotNull(result);
            Assert.Equal("María", result.Name);
            Assert.Equal("87654321", result.TaxDocument);
        }

        // Test 3: GetAllClientsAsync - Complexity 1
        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnAllClients()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);

            context.Clients.AddRange(
                new Client { Id = 1, Name = "Juan", LastName = "Pérez", TaxDocument = "111", Status = 1, LastUpdate = DateTime.Now },
                new Client { Id = 2, Name = "María", LastName = "González", TaxDocument = "222", Status = 1, LastUpdate = DateTime.Now }
            );
            await context.SaveChangesAsync();
            var result = await service.GetAllClientsAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // Test 4: CreateClientAsync - Complexity 2 - Path 1 (Valid)
        [Fact]
        public async Task CreateClientAsync_ShouldCreateClient_WhenDataIsValid()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            var clientDto = new CreateClientDTO
            {
                Name = "Pedro",
                LastName = "Ramírez",
                TaxDocument = "99999999"
            };

            createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateClientDTO>(), default))
                .ReturnsAsync(new ValidationResult());

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.CreateClientAsync(clientDto);
            Assert.NotNull(result);
            Assert.Equal("Pedro", result.Name);
            Assert.Equal("Ramírez", result.LastName);
            Assert.Equal(1, result.Status);
        }

        // Test 5: CreateClientAsync - Complexity 2 - Path 2 (Invalid)
        [Fact]
        public async Task CreateClientAsync_ShouldReturnNull_WhenValidationFails()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            var clientDto = new CreateClientDTO
            {
                Name = "",
                LastName = "Ramírez",
                TaxDocument = "99999999"
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required")
            };
            createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateClientDTO>(), default))
                .ReturnsAsync(new ValidationResult(validationFailures));

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.CreateClientAsync(clientDto);
            Assert.Null(result);
        }

        // Test 6: UpdateClientAsync - Complexity 3 - Path 1 (Valid update)
        [Fact]
        public async Task UpdateClientAsync_ShouldUpdateClient_WhenDataIsValid()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            var existingClient = new Client
            {
                Id = 1,
                Name = "Carlos",
                LastName = "López",
                TaxDocument = "11111111",
                Status = 1,
                LastUpdate = DateTime.Now
            };
            context.Clients.Add(existingClient);
            await context.SaveChangesAsync();

            var updateDto = new UpdateClientDTO
            {
                Id = 1,
                Name = "Carlos Actualizado",
                LastName = "López",
                TaxDocument = "11111111",
                Status = 1
            };

            updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), default))
                .ReturnsAsync(new ValidationResult());

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.UpdateClientAsync(updateDto);
            Assert.NotNull(result);
            Assert.Equal("Carlos Actualizado", result.Name);
        }

        // Test 7: UpdateClientAsync - Complexity 3 - Path 2 (Validation fails)
        [Fact]
        public async Task UpdateClientAsync_ShouldReturnNull_WhenValidationFails()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            var updateDto = new UpdateClientDTO
            {
                Id = 1,
                Name = "",
                LastName = "López",
                TaxDocument = "11111111",
                Status = 1
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required")
            };
            updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), default))
                .ReturnsAsync(new ValidationResult(validationFailures));

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.UpdateClientAsync(updateDto);
            Assert.Null(result);
        }

        // Test 8: UpdateClientAsync - Complexity 3 - Path 3 (Client not found)
        [Fact]
        public async Task UpdateClientAsync_ShouldReturnNull_WhenClientNotFound()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            var updateDto = new UpdateClientDTO
            {
                Id = 999,
                Name = "Inexistente",
                LastName = "Cliente",
                TaxDocument = "00000000",
                Status = 1
            };

            updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), default))
                .ReturnsAsync(new ValidationResult());

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.UpdateClientAsync(updateDto);
            Assert.Null(result);
        }

        // Test 9: DeleteClientAsync - Complexity 2 - Path 1 (Client exists)
        [Fact]
        public async Task DeleteClientAsync_ShouldReturnTrue_WhenClientExists()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            var client = new Client
            {
                Id = 1,
                Name = "Borrar",
                LastName = "Cliente",
                TaxDocument = "55555555",
                Status = 1,
                LastUpdate = DateTime.Now
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.DeleteClientAsync(1);
            Assert.True(result);
            Assert.Null(await context.Clients.FindAsync(1));
        }

        // Test 10: DeleteClientAsync - Complexity 2 - Path 2 (Client not found)
        [Fact]
        public async Task DeleteClientAsync_ShouldReturnFalse_WhenClientNotFound()
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.DeleteClientAsync(999);
            Assert.False(result);
        }
    }
}