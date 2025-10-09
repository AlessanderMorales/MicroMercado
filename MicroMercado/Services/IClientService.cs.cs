using MicroMercado.DTOs;
using MicroMercado.Models;

namespace MicroMercado.Services
{
    public interface IClientService
    {
        Task<Client?> GetClientByIdAsync(int id);
        Task<Client?> GetClientByTaxDocumentAsync(string taxDocument);
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client?> CreateClientAsync(CreateClientDTO clientDto);
        Task<Client?> UpdateClientAsync(UpdateClientDTO clientDto);
        Task<bool> DeleteClientAsync(int id);
    }
}