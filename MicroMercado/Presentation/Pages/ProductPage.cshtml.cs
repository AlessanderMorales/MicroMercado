using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using MicroMercado.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MicroMercado.Presentation.Pages;

public class ProductModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductModel> _logger;

    public List<ProductDTO> Products { get; set; } = new List<ProductDTO>();

    public ProductModel(IProductService productService, ILogger<ProductModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Entrando a OnGetAsync");
            Products = (await _productService.GetAllProductsAsync()).ToList();
            _logger.LogInformation("Productos cargados: {Count}", Products.Count);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la lista de productos");
            TempData["ErrorMessage"] = "Error al cargar la lista de productos.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var success = await _productService.DeleteProductAsync((short)id);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Producto eliminado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo eliminar el producto.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el producto {ProductId}", id);
            TempData["ErrorMessage"] = "Error al eliminar el producto.";
        }

        return RedirectToPage();
    }
}