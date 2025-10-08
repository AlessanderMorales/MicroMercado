using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroMercado.Services;
using MicroMercado.DTOs;
using System.Text.Json;

namespace MicroMercado.Pages
{
    public class SalesModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<SalesModel> _logger;

        public SalesModel(
            IProductService productService,
            ILogger<SalesModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public void OnGet()
        {
            // Inicialización de la página
        }
        
        public async Task<IActionResult> OnGetSearchProductsAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return new JsonResult(new { success = false, message = "Término de búsqueda vacío" });
                }

                var products = await _productService.SearchProductsAsync(term);
                
                return new JsonResult(new 
                { 
                    success = true, 
                    data = products.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        brand = p.Brand,
                        price = p.Price,
                        stock = p.Stock,
                        categoryId = p.CategoryId,
                        categoryName = p.CategoryName,
                        hasStock = p.HasStock,
                        label = $"{p.Name} - {p.Brand} (Stock: {p.Stock})",
                        value = p.Name
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de productos");
                return new JsonResult(new 
                { 
                    success = false, 
                    message = "Error al buscar productos" 
                });
            }
        }
        
        public async Task<IActionResult> OnGetProductByIdAsync(short id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return new JsonResult(new 
                    { 
                        success = false, 
                        message = "Producto no encontrado" 
                    });
                }

                return new JsonResult(new 
                { 
                    success = true, 
                    data = new
                    {
                        id = product.Id,
                        name = product.Name,
                        description = product.Description,
                        brand = product.Brand,
                        price = product.Price,
                        stock = product.Stock,
                        categoryId = product.CategoryId,
                        categoryName = product.CategoryName,
                        hasStock = product.HasStock
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {ProductId}", id);
                return new JsonResult(new 
                { 
                    success = false, 
                    message = "Error al obtener producto" 
                });
            }
        }
        
        public async Task<IActionResult> OnGetCheckStockAsync(short productId, short quantity)
        {
            try
            {
                var hasStock = await _productService.HasStockAsync(productId, quantity);
                
                return new JsonResult(new 
                { 
                    success = true, 
                    hasStock = hasStock 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error al verificar stock del producto {ProductId}", 
                    productId);
                return new JsonResult(new 
                { 
                    success = false, 
                    message = "Error al verificar stock" 
                });
            }
        }
    }
}