using FluentValidation;
using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MicroMercado.Presentation.Pages;

public class EditProductModel : PageModel
{ 
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<EditProductModel> _logger;
    private readonly IValidator<UpdateProductDTO> _validator;

    [BindProperty]
    public UpdateProductDTO EditProduct { get; set; } = new UpdateProductDTO();

    public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

    public EditProductModel(
        IProductService productService,
        ICategoryService categoryService,
        ILogger<EditProductModel> logger,
        IValidator<UpdateProductDTO> validator)
    {
        _productService = productService;
        _categoryService = categoryService;
        _logger = logger;
        _validator = validator;
    }

    public async Task<IActionResult> OnGetAsync(short id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid product ID: {Id}", id);
            return RedirectToPage("/ProductPage");
        }

        await LoadCategoriesAsync();

        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {Id}", id);
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToPage("/ProductPage");
            }

            // Mapear ProductDTO a UpdateProductDTO
            EditProduct = new UpdateProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product with ID: {Id}", id);
            TempData["ErrorMessage"] = "Error al cargar el producto.";
            return RedirectToPage("/ProductPage");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();
        
        ModelState.Clear();
        
        var validationResult = await _validator.ValidateAsync(EditProduct);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            
            _logger.LogWarning("Validation errors: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        
            return Page();
        }
        
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var updatedProduct = await _productService.UpdateProductAsync(EditProduct);

            if (updatedProduct == null)
            {
                ModelState.AddModelError(string.Empty, 
                    "No se pudo actualizar el producto. Verifique los datos.");
                _logger.LogWarning("Failed to update product. Data: {EditProduct}", 
                    System.Text.Json.JsonSerializer.Serialize(EditProduct));
                return Page();
            }

            TempData["SuccessMessage"] = $"¡Producto '{updatedProduct.Name}' actualizado exitosamente!";
            _logger.LogInformation("Product {ProductId} updated successfully", updatedProduct.Id);
            
            return RedirectToPage("/ProductPage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {Id}", EditProduct.Id);
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al actualizar el producto.");
            return Page();
        }
    }
    
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            Categories = allCategories.Where(c => c.Status == 1).ToList();
            
            if (!Categories.Any())
            {
                _logger.LogWarning("No active categories found");
                ModelState.AddModelError(string.Empty, 
                    "No hay categorías activas disponibles. Por favor, cree una categoría primero.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
            ModelState.AddModelError(string.Empty, "Error al cargar las categorías.");
        }
    }
}