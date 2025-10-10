using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroMercado.Services; // Necesario para IClientService
using MicroMercado.DTOs; // Necesario para CreateClientDTO
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Para el logger
using System; // Para Exception

namespace MicroMercado.Pages // <-- ¡¡¡AQUÍ!!! El namespace debe ser MicroMercado.Pages
{
    public class NewClientModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly ILogger<NewClientModel> _logger;

        // Propiedad que se vinculará automáticamente a los datos del formulario
        [BindProperty]
        public CreateClientDTO NewClient { get; set; } = new CreateClientDTO();

        public NewClientModel(IClientService clientService, ILogger<NewClientModel> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        public void OnGet()
        {
            // Este método se ejecuta al cargar la página por primera vez (GET request).
            // No necesita lógica específica para la creación de clientes aquí.
        }

        // Este método se ejecuta cuando el formulario se envía (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Errores de validación al intentar crear un nuevo cliente. Datos: {NewClient}",
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

                // Redirige a la página de detalles del cliente recién creado (ajusta la ruta si es necesario)
                return RedirectToPage("/Sales");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el cliente.");
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al crear el cliente.");
                return Page();
            }
        }
    }
}
