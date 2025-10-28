using FluentValidation; // Asegúrate de tener FluentValidation instalado
using MicroMercado.Application.DTOs.Client; // Asegúrate de que tus DTOs estén en este namespace
using MicroMercado.Application.Services; // Asegúrate de que tus servicios estén aquí
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // Para el logger
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MicroMercado.Presentation.Pages
{
    public class EditClientModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly ILogger<EditClientModel> _logger;
        private readonly IValidator<UpdateClientDTO> _validator; // Inyecta el validador para UpdateClientDTO

        [BindProperty]
        public UpdateClientDTO EditClient { get; set; } = new UpdateClientDTO();

        public EditClientModel(
            IClientService clientService,
            ILogger<EditClientModel> logger,
            IValidator<UpdateClientDTO> validator) // Constructor para inyección de dependencias
        {
            _clientService = clientService;
            _logger = logger;
            _validator = validator;
        }

        // Se ejecuta cuando la página es solicitada (GET request)
        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID de cliente inválido recibido para edición: {Id}", id);
                TempData["ErrorMessage"] = "ID de cliente no válido.";
                return RedirectToPage("/ClientPage"); // Redirige a una página de lista de clientes
            }

            try
            {
                // Asumiendo que GetClientByIdAsync devuelve un ClientDTO con los campos actualizados
                var client = await _clientService.GetClientByIdAsync(id);

                if (client == null)
                {
                    _logger.LogWarning("Cliente no encontrado con ID: {Id}", id);
                    TempData["ErrorMessage"] = "Cliente no encontrado.";
                    return RedirectToPage("/ClientPage");
                }

                // Mapear ClientDTO a UpdateClientDTO para pre-llenar el formulario
                EditClient = new UpdateClientDTO
                {
                    Id = client.Id,
                    BusinessName = client.BusinessName,
                    Email = client.Email,
                    Address = client.Address,
                    TaxDocument = client.TaxDocument,
                    Status = client.Status // Mantiene el estado actual del cliente
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el cliente con ID: {Id}", id);
                TempData["ErrorMessage"] = "Error al cargar el cliente para edición.";
                return RedirectToPage("/ClientPage");
            }
        }

        // Se ejecuta cuando el formulario es enviado (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            // Limpia el ModelState para evitar que errores antiguos interfieran
            // y para que FluentValidation haga una validación fresca.
            ModelState.Clear();

            // Realiza la validación con FluentValidation
            var validationResult = await _validator.ValidateAsync(EditClient);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                _logger.LogWarning("Errores de validación al intentar actualizar un cliente. Cliente: {EditClient}",
                    System.Text.Json.JsonSerializer.Serialize(EditClient));

                return Page(); // Vuelve a mostrar la página con errores de validación
            }

            try
            {
                var updatedClient = await _clientService.UpdateClientAsync(EditClient);

                if (updatedClient == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "No se pudo actualizar el cliente. Verifique los datos o si el documento ya está en uso.");
                    _logger.LogWarning("Fallo al actualizar cliente. Datos: {EditClient}",
                        System.Text.Json.JsonSerializer.Serialize(EditClient));
                    return Page();
                }

                TempData["SuccessMessage"] = $"¡Cliente '{updatedClient.BusinessName}' actualizado exitosamente!";
                _logger.LogInformation("Cliente {ClientId} actualizado exitosamente", updatedClient.Id);

                // Redirige a la página de lista de clientes o a la página de detalles del cliente
                return RedirectToPage("/ClientPage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el cliente con ID: {Id}", EditClient.Id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al actualizar el cliente.");
                return Page(); // Vuelve a mostrar la página con el error inesperado
            }
        }
    }
}