using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroMercado.Presentation.Pages
{
    public class CategoryPageModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryPageModel> _logger;

        public CategoryPageModel(ICategoryService categoryService, ILogger<CategoryPageModel> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Categories = (await _categoryService.GetAllCategoriesAsync()).ToList();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories list.");
                ErrorMessage = "Error al cargar la lista de categor�as.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(byte id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    ErrorMessage = "Categor�a no encontrada para eliminar.";
                    return RedirectToPage();
                }

                bool deleted = await _categoryService.DeleteCategoryAsync(id);
                if (deleted)
                {
                    SuccessMessage = $"Categor�a '{category.Name}' eliminada correctamente.";
                }
                else
                {
                    ErrorMessage = "No se pudo eliminar la categor�a.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la categor�a con ID: {CategoryId}", id);
                ErrorMessage = "Ocurri� un error inesperado al eliminar la categor�a.";
            }

            return RedirectToPage();
        }
    }
}
