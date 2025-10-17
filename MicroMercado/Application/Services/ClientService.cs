using FluentValidation;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Domain.Models; // Asegúrate de que este using esté presente para tu modelo Client
using MicroMercado.Infrastructure.Data; // Asumiendo que tu ApplicationDbContext está aquí
using Microsoft.EntityFrameworkCore; // Necesario para FindAsync, FirstOrDefaultAsync, AnyAsync, ToListAsync
using System; // Necesario para DateTime
using System.Collections.Generic; // Necesario para IEnumerable
using System.Linq; // Necesario para .Select()
using System.Threading.Tasks;

namespace MicroMercado.Application.Services
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
                // Para depuración: el logger sería mejor aquí que Console.WriteLine
                Console.WriteLine($"Validation errors creating client: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                return null;
            }

            // Verificar si ya existe un cliente con el mismo documento tributario
            var existingClient = await _context.Clients
                .AnyAsync(c => c.TaxDocument == clientDto.TaxDocument);

            if (existingClient)
            {
                Console.WriteLine($"Client with TaxDocument {clientDto.TaxDocument} already exists.");
                return null; // Podrías retornar un DTO de resultado de operación más específico
            }

            var client = new Client // Aquí estamos creando una entidad de dominio Client
            {
                // *** CAMBIOS AQUÍ: Usar BusinessName del DTO y asignarlo a BusinessName del modelo ***
                BusinessName = clientDto.BusinessName,
                // Asegúrate de que el DTO también tiene Email y Address si los necesitas aquí
                Email = clientDto.Email,
                Address = clientDto.Address,
                // **********************************************************************************
                TaxDocument = clientDto.TaxDocument,
                Status = 1, // Por defecto activo
                LastUpdate = DateTime.Now
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client; // Retorna la entidad de dominio Client
        }

        public async Task<Client?> UpdateClientAsync(UpdateClientDTO clientDto)
        {
            var validationResult = await _updateClientValidator.ValidateAsync(clientDto);
            if (!validationResult.IsValid)
            {
                // Para depuración: el logger sería mejor aquí que Console.WriteLine
                Console.WriteLine($"Validation errors updating client: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                return null;
            }

            var clientToUpdate = await _context.Clients.FindAsync(clientDto.Id);
            if (clientToUpdate == null)
            {
                return null;
            }

            // *** CAMBIOS AQUÍ: Usar BusinessName del DTO y asignarlo a BusinessName del modelo ***
            clientToUpdate.BusinessName = clientDto.BusinessName;
            // Asegúrate de que el DTO también tiene Email y Address si los necesitas aquí
            clientToUpdate.Email = clientDto.Email;
            clientToUpdate.Address = clientDto.Address;
            // **********************************************************************************
            clientToUpdate.TaxDocument = clientDto.TaxDocument;
            clientToUpdate.Status = clientDto.Status;
            clientToUpdate.LastUpdate = DateTime.Now; // Actualizar la fecha de última modificación

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