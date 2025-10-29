using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MicroMercado.Presentation.Pages
{
    public class NewCategoryModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<NewCategoryModel> _logger;

        public NewCategoryModel(ICategoryService categoryService, ILogger<NewCategoryModel> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [BindProperty]
        public CreateCategoryDTO CreateCategory { get; set; } = new CreateCategoryDTO();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var result = await _categoryService.CreateCategoryAsync(CreateCategory);
                if (result == null)
                {
                    ErrorMessage = "No se pudo crear la categoría. Verifique los datos o que no exista una categoría con el mismo nombre.";
                    return Page();
                }

                SuccessMessage = $"Categoría '{result.Name}' creada correctamente.";
                return RedirectToPage("/Category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando categoría.");
                ErrorMessage = "Ocurrió un error inesperado al crear la categoría.";
                return Page();
            }
        }
    }
}
