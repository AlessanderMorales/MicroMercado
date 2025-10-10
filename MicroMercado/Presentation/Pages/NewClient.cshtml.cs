using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
// Necesario para IClientService
// Necesario para CreateClientDTO
// Para el logger

// Para Exception

namespace MicroMercado.Presentation.Pages // <-- ���AQU�!!! El namespace debe ser MicroMercado.Pages
{
    public class NewClientModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly ILogger<NewClientModel> _logger;

        // Propiedad que se vincular� autom�ticamente a los datos del formulario
        [BindProperty]
        public CreateClientDTO NewClient { get; set; } = new CreateClientDTO();

        public NewClientModel(IClientService clientService, ILogger<NewClientModel> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        public void OnGet()
        {
            // Este m�todo se ejecuta al cargar la p�gina por primera vez (GET request).
            // No necesita l�gica espec�fica para la creaci�n de clientes aqu�.
        }

        // Este m�todo se ejecuta cuando el formulario se env�a (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Errores de validaci�n al intentar crear un nuevo cliente. Datos: {NewClient}",
                    System.Text.Json.JsonSerializer.Serialize(NewClient));
                return Page();
            }

            try
            {
                var createdClient = await _clientService.CreateClientAsync(NewClient);

                if (createdClient == null)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo crear el cliente. Verifique los datos o si ya existe un cliente con ese documento.");
                    _logger.LogWarning("No se pudo crear el cliente. Datos: {NewClient}", System.Text.Json.JsonSerializer.Serialize(NewClient));
                    return Page();
                }

                // Redirige a la p�gina de detalles del cliente reci�n creado (ajusta la ruta si es necesario)
                return RedirectToPage("/Sales");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el cliente.");
                ModelState.AddModelError(string.Empty, "Ocurri� un error inesperado al crear el cliente.");
                return Page();
            }
        }
    }
}
