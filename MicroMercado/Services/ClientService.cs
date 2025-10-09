using Microsoft.EntityFrameworkCore;
using MicroMercado.Data;
using MicroMercado.DTOs;
using MicroMercado.Models;
using MicroMercado.Validators.Client; 
using FluentValidation;

namespace MicroMercado.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateClientDTO> _createClientValidator; 
        private readonly IValidator<UpdateClientDTO> _updateClientValidator; 

        public ClientService(ApplicationDbContext context,
                             IValidator<CreateClientDTO> createClientValidator, 
                             IValidator<UpdateClientDTO> updateClientValidator) 
        {
            _context = context;
            _createClientValidator = createClientValidator;
            _updateClientValidator = updateClientValidator;
        }

        public async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task<Client?> GetClientByTaxDocumentAsync(string taxDocument)
        {
            return await _context.Clients.FirstOrDefaultAsync(c => c.TaxDocument == taxDocument);
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _context.Clients.ToListAsync();
        }
        public async Task<Client?> CreateClientAsync(CreateClientDTO clientDto)
        {
            var validationResult = await _createClientValidator.ValidateAsync(clientDto);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Validation errors creating client: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                return null;
            }

            var client = new Client
            {
                Name = clientDto.Name,
                LastName = clientDto.LastName,
                TaxDocument = clientDto.TaxDocument,
                Status = 1,
                LastUpdate = DateTime.UtcNow
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Client?> UpdateClientAsync(UpdateClientDTO clientDto)
        {
            var validationResult = await _updateClientValidator.ValidateAsync(clientDto);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Validation errors updating client: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                return null;
            }

            var clientToUpdate = await _context.Clients.FindAsync(clientDto.Id);
            if (clientToUpdate == null)
            {
                return null;
            }

            clientToUpdate.Name = clientDto.Name;
            clientToUpdate.LastName = clientDto.LastName;
            clientToUpdate.TaxDocument = clientDto.TaxDocument;
            clientToUpdate.Status = clientDto.Status;
            clientToUpdate.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return clientToUpdate;
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