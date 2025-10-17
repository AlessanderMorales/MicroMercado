using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace MicroMercado.Presentation.Pages
{
    public class NewClientModel : PageModel
    {
        private readonly IClientService _clientService;
        private readonly ILogger<NewClientModel> _logger;
        [BindProperty]
        public CreateClientDTO NewClient { get; set; } = new CreateClientDTO(); 

        public NewClientModel(IClientService clientService, ILogger<NewClientModel> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        public void OnGet()
        {
            // ...
        }

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