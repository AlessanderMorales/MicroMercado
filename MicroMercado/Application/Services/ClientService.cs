using FluentValidation;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Domain.Models; 
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 
using System.Linq; 

namespace MicroMercado.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateClientDTO> _createClientValidator;
        private readonly IValidator<UpdateClientDTO> _updateClientValidator;
        private readonly ILogger<ClientService> _logger; 

        public ClientService(ApplicationDbContext context,
                             IValidator<CreateClientDTO> createClientValidator,
                             IValidator<UpdateClientDTO> updateClientValidator,
                             ILogger<ClientService> logger)
        {
            _context = context;
            _createClientValidator = createClientValidator;
            _updateClientValidator = updateClientValidator;
            _logger = logger; 
        }

        public async Task<ClientDTO?> GetClientByIdAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return null;
            }

            return new ClientDTO
            {
                Id = client.Id,
                BusinessName = client.BusinessName,
                Email = client.Email,
                Address = client.Address,
                TaxDocument = client.TaxDocument,
                Status = (byte)client.Status,
                LastUpdate = DateTime.SpecifyKind(client.LastUpdate, DateTimeKind.Unspecified) 
            };
        }

        public async Task<ClientDTO?> GetClientByTaxDocumentAsync(string taxDocument)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.TaxDocument == taxDocument);
            if (client == null)
            {
                return null;
            }
            return new ClientDTO
            {
                Id = client.Id,
                BusinessName = client.BusinessName,
                Email = client.Email,
                Address = client.Address,
                TaxDocument = client.TaxDocument,
                Status = (byte)client.Status,
                LastUpdate = DateTime.SpecifyKind(client.LastUpdate, DateTimeKind.Unspecified)
            };
        }

        public async Task<IEnumerable<ClientDTO>> GetAllClientsAsync()
        {
            return await _context.Clients
                .Select(client => new ClientDTO 
                {
                    Id = client.Id,
                    BusinessName = client.BusinessName,
                    Email = client.Email,
                    Address = client.Address,
                    TaxDocument = client.TaxDocument,
                    Status = (byte)client.Status,
                    LastUpdate = DateTime.SpecifyKind(client.LastUpdate, DateTimeKind.Unspecified)
                })
                .ToListAsync();
        }
        public async Task<ClientDTO?> CreateClientAsync(CreateClientDTO clientDto)
        {
            var validationResult = await _createClientValidator.ValidateAsync(clientDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation errors creating client: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return null;
            }
            var existingClient = await _context.Clients
                .AnyAsync(c => c.TaxDocument == clientDto.TaxDocument);

            if (existingClient)
            {
                _logger.LogWarning("Client with TaxDocument {TaxDocument} already exists.", clientDto.TaxDocument);
                return null;
            }

            var client = new Client
            {
                BusinessName = clientDto.BusinessName,
                Email = clientDto.Email,
                Address = clientDto.Address,
                TaxDocument = clientDto.TaxDocument,
                Status = 1,
                LastUpdate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return new ClientDTO
            {
                Id = client.Id,
                BusinessName = client.BusinessName,
                Email = client.Email,
                Address = client.Address,
                TaxDocument = client.TaxDocument,
                Status = (byte)client.Status,
                LastUpdate = DateTime.SpecifyKind(client.LastUpdate, DateTimeKind.Unspecified)
            };
        }
        public async Task<ClientDTO?> UpdateClientAsync(UpdateClientDTO clientDto)
        {
            var validationResult = await _updateClientValidator.ValidateAsync(clientDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation errors updating client: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return null;
            }

            var clientToUpdate = await _context.Clients.FindAsync(clientDto.Id);
            if (clientToUpdate == null)
            {
                return null;
            }

            clientToUpdate.BusinessName = clientDto.BusinessName;
            clientToUpdate.Email = clientDto.Email;
            clientToUpdate.Address = clientDto.Address;
            clientToUpdate.TaxDocument = clientDto.TaxDocument;
            clientToUpdate.Status = clientDto.Status;
            clientToUpdate.LastUpdate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return new ClientDTO
            {
                Id = clientToUpdate.Id,
                BusinessName = clientToUpdate.BusinessName,
                Email = clientToUpdate.Email,
                Address = clientToUpdate.Address,
                TaxDocument = clientToUpdate.TaxDocument,
                Status = (byte)clientToUpdate.Status,
                LastUpdate = DateTime.SpecifyKind(clientToUpdate.LastUpdate, DateTimeKind.Unspecified)
            };
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var clientToDelete = await _context.Clients.FindAsync(id);
            if (clientToDelete == null)
            {
                return false;
            }

            _context.Clients.Remove(clientToDelete);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}