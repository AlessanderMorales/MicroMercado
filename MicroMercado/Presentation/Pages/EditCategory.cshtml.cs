using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MicroMercado.Presentation.Pages
{
    public class EditCategoryModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<EditCategoryModel> _logger;

        public EditCategoryModel(ICategoryService categoryService, ILogger<EditCategoryModel> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [BindProperty]
        public UpdateCategoryDTO UpdateCategory { get; set; } = new UpdateCategoryDTO();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(byte id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                ErrorMessage = "Categoría no encontrada.";
                return RedirectToPage("/Category");
            }

            UpdateCategory.Id = category.Id;
            UpdateCategory.Name = category.Name;
            UpdateCategory.Description = category.Description;
            UpdateCategory.Status = category.Status;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var result = await _categoryService.UpdateCategoryAsync(UpdateCategory);
                if (result == null)
                {
                    ErrorMessage = "No se pudo actualizar la categoría. Verifique los datos o que no exista otra categoría con el mismo nombre.";
                    return Page();
                }

                SuccessMessage = $"Categoría '{result.Name}' actualizada correctamente.";
                return RedirectToPage("/Category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando categoría.");
                ErrorMessage = "Ocurrió un error inesperado al actualizar la categoría.";
                return Page();
            }
        }
    }
}
