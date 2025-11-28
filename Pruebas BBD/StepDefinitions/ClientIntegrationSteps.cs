using FluentValidation;
using MicroMercado.Application.DTOs.Client;
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
    public class ClientIntegrationSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private ApplicationDbContext _context;
        private IClientService _clientService;
        private ClientDTO? _resultClient;
        private Exception? _exception;
        private static int _nextClientId = 1;

        public ClientIntegrationSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _nextClientId = 1;
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
                .AddScoped<IClientService, ClientService>()
                .AddScoped<IValidator<CreateClientDTO>, MicroMercado.Application.Validators.Client.CreateClientValidator>()
                .AddScoped<IValidator<UpdateClientDTO>, MicroMercado.Application.Validators.Client.UpdateClientValidator>()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            _clientService = serviceProvider.GetRequiredService<IClientService>();
        }

        [When(@"creo un cliente con los siguientes datos:")]
        public async Task WhenCreoUnCliente(Table table)
        {
            try
            {
                string businessName = string.Empty;
                string email = string.Empty;
                string taxDocument = string.Empty;
                string address = string.Empty;

                foreach (var row in table.Rows)
                {
                    var campo = row["Campo"];
                    var valor = row["Valor"];

                    switch (campo)
                    {
                        case "BusinessName":
                            businessName = valor;
                            break;
                        case "Email":
                            email = valor;
                            break;
                        case "TaxDocument":
                            taxDocument = valor;
                            break;
                        case "Address":
                            address = valor;
                            break;
                    }
                }

                var dto = new CreateClientDTO
                {
                    BusinessName = businessName,
                    Email = email,
                    TaxDocument = taxDocument,
                    Address = address
                };

                _resultClient = await _clientService.CreateClientAsync(dto);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then(@"el cliente debe crearse exitosamente")]
        public void ThenElClienteDebeCrearseExitosamente()
        {
            Assert.NotNull(_resultClient);
            Assert.Null(_exception);
        }

        [Then(@"el TaxDocument debe ser unico")]
        public async Task ThenElTaxDocumentDebeSerUnico()
        {
            var duplicates = await _context.Clients
                .Where(c => c.TaxDocument == _resultClient!.TaxDocument)
                .ToListAsync();
            Assert.Equal(1, duplicates.Count);
        }

        [Then(@"el Email debe ser unico")]
        public async Task ThenElEmailDebeSerUnico()
        {
            var duplicates = await _context.Clients
                .Where(c => c.Email == _resultClient!.Email)
                .ToListAsync();
            Assert.Equal(1, duplicates.Count);
        }

        [When(@"intento crear un cliente con email ""(.*)""")]
        public async Task WhenIntentoCrearUnClienteConEmail(string email)
        {
            try
            {
                var dto = new CreateClientDTO
                {
                    BusinessName = "Cliente Test",
                    Email = email,
                    TaxDocument = "12345678",
                    Address = "Dirección"
                };

                _resultClient = await _clientService.CreateClientAsync(dto);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then(@"debe mostrar error de validacion de email")]
        public void ThenDebeMostrarErrorDeValidacionDeEmail()
        {
            Assert.Null(_resultClient);
        }

        [Given(@"existe un cliente con TaxDocument ""(.*)""")]
        public async Task GivenExisteUnClienteConTaxDocument(string taxDocument)
        {
            _context.ChangeTracker.Clear();
            
            var client = new Client
            {
                Id = _nextClientId++,
                BusinessName = "Cliente Existente",
                Email = "cliente@example.com",
                TaxDocument = taxDocument,
                Address = "Dirección inicial",
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _scenarioContext["ClientId"] = client.Id;
        }

        [When(@"actualizo el cliente con:")]
        public async Task WhenActualizoElClienteCon(Table table)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var clientId = (int)_scenarioContext["ClientId"];
                var existingClient = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);
                
                if (existingClient == null)
                    throw new InvalidOperationException($"Client with ID {clientId} not found");

                string businessName = existingClient.BusinessName;
                string email = existingClient.Email;
                string taxDocument = existingClient.TaxDocument;
                string address = existingClient.Address ?? string.Empty;
                byte status = existingClient.Status;

                foreach (var row in table.Rows)
                {
                    var campo = row["Campo"];
                    var valor = row["Valor"];

                    switch (campo)
                    {
                        case "BusinessName":
                            businessName = valor;
                            break;
                        case "Email":
                            email = valor;
                            break;
                        case "TaxDocument":
                            taxDocument = valor;
                            break;
                        case "Address":
                            address = valor;
                            break;
                        case "Status":
                            status = byte.Parse(valor);
                            break;
                    }
                }

                var dto = new UpdateClientDTO
                {
                    Id = clientId,
                    BusinessName = businessName,
                    Email = email,
                    TaxDocument = taxDocument,
                    Address = address,
                    Status = status
                };

                _resultClient = await _clientService.UpdateClientAsync(dto);
                _scenarioContext["LastUpdateSuccess"] = _resultClient != null;
                _scenarioContext["LastUpdatedClientId"] = _resultClient?.Id;
            }
            catch (Exception ex)
            {
                _exception = ex;
                _scenarioContext["LastUpdateSuccess"] = false;
                _scenarioContext["LastException"] = ex;
            }
        }

        [Then(@"los datos deben estar actualizados en BD")]
        public async Task ThenLosDatosDebenEstarActualizadosEnBD()
        {
            var dbClient = await _context.Clients.FindAsync(_resultClient?.Id);
            Assert.NotNull(dbClient);
            Assert.Equal(_resultClient?.BusinessName, dbClient.BusinessName);
        }

        [Given(@"existe un cliente ""(.*)"" con email ""(.*)""")]
        public async Task GivenExisteUnClienteConEmail(string nombre, string email)
        {
            _context.ChangeTracker.Clear();
            
            var client = new Client
            {
                Id = _nextClientId++,
                BusinessName = nombre,
                Email = email,
                TaxDocument = "11111111",
                Address = "Dirección",
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _scenarioContext["ClientId"] = client.Id;
        }

        [When(@"actualizo el email a ""(.*)""")]
        public async Task WhenActualizoElEmailA(string nuevoEmail)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var clientId = (int)_scenarioContext["ClientId"];
                var existingClient = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);

                var dto = new UpdateClientDTO
                {
                    Id = clientId,
                    BusinessName = existingClient!.BusinessName,
                    Email = nuevoEmail,
                    TaxDocument = existingClient.TaxDocument,
                    Address = existingClient.Address,
                    Status = existingClient.Status
                };

                _resultClient = await _clientService.UpdateClientAsync(dto);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then(@"el nuevo email debe ser unico")]
        public async Task ThenElNuevoEmailDebeSerUnico()
        {
            var duplicates = await _context.Clients
                .Where(c => c.Email == _resultClient!.Email)
                .ToListAsync();
            Assert.Equal(1, duplicates.Count);
        }

        [Given(@"existen los siguientes clientes:")]
        public async Task GivenExistenLosSiguientesClientes(Table table)
        {
            _context.ChangeTracker.Clear();
            
            foreach (var row in table.Rows)
            {
                var client = new Client
                {
                    Id = _nextClientId++,
                    BusinessName = row["BusinessName"],
                    Email = $"{row["BusinessName"].Replace(" ", "")}@example.com",
                    TaxDocument = row["TaxDocument"],
                    Address = "Dirección",
                    Status = 1,
                    LastUpdate = DateTime.Now
                };

                _context.Clients.Add(client);
            }
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        [When(@"intento actualizar ""(.*)"" con TaxDocument ""(.*)""")]
        public async Task WhenIntentoActualizarConTaxDocument(string nombreCliente, string nuevoTaxDocument)
        {
            try
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.BusinessName == nombreCliente);

                var dto = new UpdateClientDTO
                {
                    Id = client!.Id,
                    BusinessName = client.BusinessName,
                    Email = client.Email,
                    TaxDocument = nuevoTaxDocument,
                    Address = client.Address,
                    Status = client.Status
                };

                _resultClient = await _clientService.UpdateClientAsync(dto);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then(@"debe mostrar error de TaxDocument duplicado")]
        public void ThenDebeMostrarErrorDeTaxDocumentDuplicado()
        {
            Assert.Null(_resultClient);
        }

        [When(@"elimino el cliente")]
        public async Task WhenEliminoElCliente()
        {
            var clientId = (int)_scenarioContext["ClientId"];
            await _clientService.DeleteClientAsync(clientId);

            var deletedClient = await _context.Clients.FindAsync(clientId);
            _resultClient = deletedClient != null ? new ClientDTO
            {
                Id = deletedClient.Id,
                BusinessName = deletedClient.BusinessName,
                Status = deletedClient.Status
            } : null;
        }

        [Given(@"existe un cliente ""(.*)"" con TaxDocument ""(.*)""")]
        public async Task GivenExisteUnClienteConNombreYTaxDocument(string nombre, string taxDocument)
        {
            _context.ChangeTracker.Clear();
            
            var client = new Client
            {
                Id = _nextClientId++,
                BusinessName = nombre,
                Email = $"{nombre.Replace(" ", "").ToLower()}@example.com",
                TaxDocument = taxDocument,
                Address = "Dirección",
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _scenarioContext["ClientId"] = client.Id;
        }

        [When(@"busco el cliente por TaxDocument ""(.*)""")]
        public async Task WhenBuscoElClientePorTaxDocument(string taxDocument)
        {
            _resultClient = await _clientService.GetClientByTaxDocumentAsync(taxDocument);
        }

        [Then(@"debo recibir los datos del cliente")]
        public void ThenDeboRecibirLosDatosDelCliente()
        {
            Assert.NotNull(_resultClient);
        }

        [Then(@"el BusinessName debe ser ""(.*)""")]
        public void ThenElBusinessNameDebeSer(string expectedName)
        {
            Assert.Equal(expectedName, _resultClient?.BusinessName);
        }

        [Then(@"debe tener Status {int}")]
        public void ThenDebeTenerStatus(int expectedStatus)
        {
            Assert.NotNull(_resultClient);
            Assert.Equal((byte)expectedStatus, _resultClient.Status);
        }

        [Then(@"el Status del cliente debe cambiar a 0")]
        public void ThenElStatusDelClienteDebeCambiarA()
        {
            Assert.NotNull(_resultClient);
            Assert.Equal((byte)0, _resultClient.Status);
        }

        [Then(@"el cliente no debe aparecer en busquedas activas")]
        public async Task ThenElClienteNoDebeAparecerEnBusquedasActivas()
        {
            var activeClients = await _clientService.GetAllClientsAsync();
            Assert.DoesNotContain(activeClients, c => c.Id == _resultClient?.Id);
        }

        [AfterScenario]
        public void CleanupDatabase()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}
