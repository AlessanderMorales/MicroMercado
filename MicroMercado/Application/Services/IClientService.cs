
using MicroMercado.Application.DTOs.Client; 

namespace MicroMercado.Application.Services
{
    public interface IClientService
    {
        Task<ClientDTO?> GetClientByIdAsync(int id);
        Task<ClientDTO?> GetClientByTaxDocumentAsync(string taxDocument);
        Task<IEnumerable<ClientDTO>> GetAllClientsAsync();
        Task<ClientDTO?> CreateClientAsync(CreateClientDTO clientDto);
        Task<ClientDTO?> UpdateClientAsync(UpdateClientDTO clientDto);
        Task<bool> DeleteClientAsync(int id);
    }
}