using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MicroMercado.Presentation.Pages.Category
{
    public class NewCategoryModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<NewCategoryModel> _logger;

        [BindProperty]
        public CreateCategoryDTO NewCategory { get; set; } = new CreateCategoryDTO();

        public NewCategoryModel(ICategoryService categoryService, ILogger<NewCategoryModel> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public void OnGet()
        {
            // aca va el metodo para mostrar el formulario
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Errores de validación al intentar crear una nueva categoría. Datos: {NewCategory}",
                    System.Text.Json.JsonSerializer.Serialize(NewCategory));
                return Page();
            }

            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(NewCategory);

                if (createdCategory == null)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo crear la categoría. Verifique los datos o si ya existe una categoría con ese nombre.");
                    _logger.LogWarning("No se pudo crear la categoría. Datos: {NewCategory}", System.Text.Json.JsonSerializer.Serialize(NewCategory));
                    return Page();
                }

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoría.");
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al crear la categoría.");
                return Page();
            }
        }
    }
}