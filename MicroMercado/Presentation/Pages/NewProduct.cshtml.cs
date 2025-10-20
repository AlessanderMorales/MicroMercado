using FluentValidation;
using MicroMercado.Application.DTOs.Category;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MicroMercado.Presentation.Pages;

public class NewProductModel : PageModel
{ 
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<NewProductModel> _logger;
    private readonly IValidator<CreateProductDTO> _validator;

    [BindProperty]
    public CreateProductDTO NewProduct { get; set; } = new CreateProductDTO();

    public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

    public NewProductModel(
        IProductService productService,
        ICategoryService categoryService,
        ILogger<NewProductModel> logger,
        IValidator<CreateProductDTO> validator)
    {
        _productService = productService;
        _logger = logger;
        _categoryService = categoryService;
        _validator = validator;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();
        
        ModelState.Clear();
        
        var validationResult = await _validator.ValidateAsync(NewProduct);

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
            var createdProduct = await _productService.CreateProductAsync(NewProduct);

            if (createdProduct == null)
            {
                ModelState.AddModelError(string.Empty, 
                    "No se pudo crear el producto. Verifique que el nombre no exista.");
                _logger.LogWarning("Failed to create product. Data: {NewProduct}", 
                    System.Text.Json.JsonSerializer.Serialize(NewProduct));
                return Page();
            }

            TempData["SuccessMessage"] = $"¡Producto '{createdProduct.Name}' creado exitosamente!";
            _logger.LogInformation("Product {ProductId} created successfully by user", createdProduct.Id);
            
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al crear el producto.");
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