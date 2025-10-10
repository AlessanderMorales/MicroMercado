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

        // Test 1: GetClientByIdAsync - Parameterized
        // Complexity: 1
        [Theory]
        [InlineData(1, "Juan", "Pérez", "12345678")]
        [InlineData(2, "María", "González", "87654321")]
        [InlineData(3, "Pedro", "Ramírez", "11111111")]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenClientExists(
            int clientId,
            string expectedName,
            string expectedLastName,
            string expectedTaxDocument)
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);

            context.Clients.AddRange(
                new Client { Id = 1, Name = "Juan", LastName = "Pérez", TaxDocument = "12345678", Status = 1, LastUpdate = DateTime.Now },
                new Client { Id = 2, Name = "María", LastName = "González", TaxDocument = "87654321", Status = 1, LastUpdate = DateTime.Now },
                new Client { Id = 3, Name = "Pedro", LastName = "Ramírez", TaxDocument = "11111111", Status = 1, LastUpdate = DateTime.Now }
            );
            await context.SaveChangesAsync();
            var result = await service.GetClientByIdAsync(clientId);
            Assert.NotNull(result);
            Assert.Equal(expectedName, result.Name);
            Assert.Equal(expectedLastName, result.LastName);
            Assert.Equal(expectedTaxDocument, result.TaxDocument);
        }

        // Test 2: GetClientByTaxDocumentAsync - Parameterized
        // Complexity: 1
        [Theory]
        [InlineData("12345678", "Juan", "Pérez")]
        [InlineData("87654321", "María", "González")]
        [InlineData("11111111", "Pedro", "Ramírez")]
        public async Task GetClientByTaxDocumentAsync_ShouldReturnClient_WhenTaxDocumentExists(
            string taxDocument,
            string expectedName,
            string expectedLastName)
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);

            context.Clients.AddRange(
                new Client { Id = 1, Name = "Juan", LastName = "Pérez", TaxDocument = "12345678", Status = 1, LastUpdate = DateTime.Now },
                new Client { Id = 2, Name = "María", LastName = "González", TaxDocument = "87654321", Status = 1, LastUpdate = DateTime.Now },
                new Client { Id = 3, Name = "Pedro", LastName = "Ramírez", TaxDocument = "11111111", Status = 1, LastUpdate = DateTime.Now }
            );
            await context.SaveChangesAsync();
            var result = await service.GetClientByTaxDocumentAsync(taxDocument);
            Assert.NotNull(result);
            Assert.Equal(expectedName, result.Name);
            Assert.Equal(expectedLastName, result.LastName);
        }

        // Test 3: GetAllClientsAsync
        // Complexity: 1
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

        // Test 4 & 5: CreateClientAsync - Parameterized
        // Complexity: 2 (Path 1: Valid, Path 2: Invalid)
        public static TheoryData<CreateClientDTO, bool, bool> CreateClientTestData =>
            new TheoryData<CreateClientDTO, bool, bool>
            {
                { new CreateClientDTO { Name = "Pedro", LastName = "Ramírez", TaxDocument = "99999999" }, true, true },
                { new CreateClientDTO { Name = "Ana", LastName = "López", TaxDocument = "88888888" }, true, true },
                { new CreateClientDTO { Name = "", LastName = "Ramírez", TaxDocument = "99999999" }, false, false },
                { new CreateClientDTO { Name = "Pedro", LastName = "", TaxDocument = "99999999" }, false, false }
            };

        [Theory]
        [MemberData(nameof(CreateClientTestData))]
        public async Task CreateClientAsync_ShouldReturnCorrectResult_BasedOnValidation(
            CreateClientDTO clientDto,
            bool isValidationSuccess,
            bool shouldReturnClient)
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            if (isValidationSuccess)
            {
                createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateClientDTO>(), default))
                    .ReturnsAsync(new ValidationResult());
            }
            else
            {
                var validationFailures = new List<ValidationFailure>
                {
                    new ValidationFailure("Name", "Name is required")
                };
                createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateClientDTO>(), default))
                    .ReturnsAsync(new ValidationResult(validationFailures));
            }

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.CreateClientAsync(clientDto);
            if (shouldReturnClient)
            {
                Assert.NotNull(result);
                Assert.Equal(clientDto.Name, result.Name);
                Assert.Equal(1, result.Status);
            }
            else
            {
                Assert.Null(result);
            }
        }

        // Test 6, 7 & 8: UpdateClientAsync - Parameterized
        // Complexity: 3 (Path 1: Valid, Path 2: Invalid, Path 3: Not Found)
        public static TheoryData<int, UpdateClientDTO, bool, bool, bool> UpdateClientTestData =>
            new TheoryData<int, UpdateClientDTO, bool, bool, bool>
            {
                { 1, new UpdateClientDTO { Id = 1, Name = "Carlos Updated", LastName = "López", TaxDocument = "11111111", Status = 1 }, true, true, true },
                { 1, new UpdateClientDTO { Id = 1, Name = "", LastName = "López", TaxDocument = "11111111", Status = 1 }, false, true, false },
                { 1, new UpdateClientDTO { Id = 999, Name = "Inexistente", LastName = "Cliente", TaxDocument = "00000000", Status = 1 }, true, false, false }
            };

        [Theory]
        [MemberData(nameof(UpdateClientTestData))]
        public async Task UpdateClientAsync_ShouldReturnCorrectResult_BasedOnScenario(
            int existingClientId,
            UpdateClientDTO updateDto,
            bool isValidationSuccess,
            bool clientExists,
            bool expectedSuccess)
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();

            if (clientExists)
            {
                var existingClient = new Client
                {
                    Id = existingClientId,
                    Name = "Carlos",
                    LastName = "López",
                    TaxDocument = "11111111",
                    Status = 1,
                    LastUpdate = DateTime.Now
                };
                context.Clients.Add(existingClient);
                await context.SaveChangesAsync();
            }

            if (isValidationSuccess)
            {
                updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), default))
                    .ReturnsAsync(new ValidationResult());
            }
            else
            {
                var validationFailures = new List<ValidationFailure>
                {
                    new ValidationFailure("Name", "Name is required")
                };
                updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClientDTO>(), default))
                    .ReturnsAsync(new ValidationResult(validationFailures));
            }

            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);
            var result = await service.UpdateClientAsync(updateDto);
            if (expectedSuccess)
            {
                Assert.NotNull(result);
                Assert.Equal(updateDto.Name, result.Name);
            }
            else
            {
                Assert.Null(result);
            }
        }

        // Test 9 & 10: DeleteClientAsync - Parameterized
        // Complexity: 2 (Path 1: Exists, Path 2: Not Exists)
        [Theory]
        [InlineData(1, true)] 
        [InlineData(999, false)]
        public async Task DeleteClientAsync_ShouldReturnCorrectResult_BasedOnClientExistence(
            int clientId,
            bool expectedResult)
        {
            var context = GetInMemoryDbContext();
            var createValidatorMock = new Mock<IValidator<CreateClientDTO>>();
            var updateValidatorMock = new Mock<IValidator<UpdateClientDTO>>();
            var service = new ClientService(context, createValidatorMock.Object, updateValidatorMock.Object);

            if (expectedResult)
            {
                var client = new Client
                {
                    Id = 1,
                    Name = "Test",
                    LastName = "Client",
                    TaxDocument = "12345678",
                    Status = 1,
                    LastUpdate = DateTime.Now
                };
                context.Clients.Add(client);
                await context.SaveChangesAsync();
            }

            var result = await service.DeleteClientAsync(clientId);
            Assert.Equal(expectedResult, result);

            if (expectedResult)
            {
                Assert.Null(await context.Clients.FindAsync(clientId));
            }
        }
    }
}
