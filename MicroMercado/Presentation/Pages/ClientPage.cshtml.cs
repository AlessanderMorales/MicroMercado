using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroMercado.Presentation.Pages
{
    public class ClientPageModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientPageModel> _logger;

        public ClientPageModel(IClientService clientService, ILogger<ClientPageModel> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        public List<ClientDTO> Clients { get; set; } = new List<ClientDTO>();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Clients = (await _clientService.GetAllClientsAsync()).ToList();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de clientes.");
                ErrorMessage = "Error al cargar la lista de clientes.";
                return Page();
            }
        }

        // Puedes añadir un handler para eliminar clientes si lo necesitas
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var clientToDelete = await _clientService.GetClientByIdAsync(id);
                if (clientToDelete == null)
                {
                    ErrorMessage = "Cliente no encontrado para eliminar.";
                    return RedirectToPage();
                }

                bool deleted = await _clientService.DeleteClientAsync(id);
                if (deleted)
                {
                    SuccessMessage = $"Cliente '{clientToDelete.BusinessName}' eliminado exitosamente.";
                }
                else
                {
                    ErrorMessage = "No se pudo eliminar el cliente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el cliente con ID: {ClientId}", id);
                ErrorMessage = "Ocurrió un error inesperado al eliminar el cliente.";
            }

            return RedirectToPage(); // Redirige de nuevo a la lista para ver los cambios
        }
    }
}