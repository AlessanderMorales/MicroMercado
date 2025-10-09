using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroMercado.Services;
using MicroMercado.DTOs.Sales;

namespace MicroMercado.Pages
{
    public class SalesModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ISaleService _saleService; 
        private readonly ILogger<SalesModel> _logger;

        public SalesModel(
            IProductService productService,
            ISaleService saleService,
            ILogger<SalesModel> logger)
        {
            _productService = productService;
            _saleService = saleService;
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
        
        
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostConfirmSaleAsync()
        {
            try
            {
                _logger.LogInformation("=== INICIO OnPostConfirmSaleAsync ===");
                _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
                _logger.LogInformation("Content-Length: {ContentLength}", Request.ContentLength);
                
                // 🔍 PASO 1: Leer el body RAW para ver qué llega
                string rawBody = "";
                using (var reader = new StreamReader(Request.Body))
                {
                    rawBody = await reader.ReadToEndAsync();
                }
                _logger.LogInformation("📦 Body RAW recibido: {RawBody}", rawBody);
                
                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    _logger.LogWarning("❌ Body está vacío");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se recibió ningún dato en el body"
                    });
                }

                // 🔍 PASO 2: Intentar deserializar manualmente
                SaleDTO.CreateSaleDTO? saleDTO = null;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = null
                    };
                    
                    saleDTO = JsonSerializer.Deserialize<SaleDTO.CreateSaleDTO>(rawBody, options);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "❌ Error al deserializar JSON");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "JSON malformado: " + jsonEx.Message
                    });
                }

                if (saleDTO == null)
                {
                    _logger.LogWarning("❌ saleDTO es null después de deserializar");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se pudo procesar el JSON recibido"
                    });
                }

                // 🔍 PASO 3: Validar datos recibidos
                _logger.LogInformation("✅ saleDTO deserializado correctamente");
                _logger.LogInformation("ClientId: {ClientId} (Type: {Type})", 
                    saleDTO.ClientId, 
                    saleDTO.ClientId?.GetType().Name ?? "null");
                _logger.LogInformation("PaymentType: {PaymentType}", saleDTO.PaymentType);
                _logger.LogInformation("TotalAmount: {TotalAmount}", saleDTO.TotalAmount);
                _logger.LogInformation("CashReceived: {CashReceived}", saleDTO.CashReceived);
                _logger.LogInformation("Change: {Change}", saleDTO.Change);
                _logger.LogInformation("Items Count: {ItemsCount}", saleDTO.Items?.Count ?? 0);

                if (saleDTO.Items != null && saleDTO.Items.Any())
                {
                    foreach (var item in saleDTO.Items)
                    {
                        _logger.LogInformation("  Item: ProductId={ProductId}, Qty={Qty}, Price={Price}, Total={Total}",
                            item.ProductId, item.Quantity, item.Price, item.Total);
                    }
                }

                if (saleDTO.Items == null || !saleDTO.Items.Any())
                {
                    _logger.LogWarning("❌ No hay items en la venta");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No hay productos para vender"
                    });
                }

                // 🔍 PASO 4: Validar total
                var calculatedTotal = saleDTO.Items.Sum(i => i.Total);
                _logger.LogInformation("Total calculado: {Calculated}, Total recibido: {Received}", 
                    calculatedTotal, saleDTO.TotalAmount);
                    
                if (Math.Abs(calculatedTotal - saleDTO.TotalAmount) > 0.01m)
                {
                    _logger.LogWarning("⚠️ Total no coincide exactamente pero continúa");
                }

                // 🔍 PASO 5: Intentar crear la venta
                _logger.LogInformation("Llamando a _saleService.CreateSaleAsync...");
                var result = await _saleService.CreateSaleAsync(saleDTO);
                _logger.LogInformation("Respuesta del servicio: Success={Success}, Message={Message}", 
                    result.Success, result.Message);

                if (result.Success)
                {
                    _logger.LogInformation("✅ Venta creada exitosamente. SaleId: {SaleId}", 
                        result.Data?.SaleId);
                    return new JsonResult(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data
                    });
                }

                _logger.LogWarning("❌ Error al crear venta: {Message}", result.Message);
                return new JsonResult(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPCIÓN en OnPostConfirmSaleAsync");
                _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
                return new JsonResult(new
                {
                    success = false,
                    message = "Error al procesar la venta: " + ex.Message,
                    error = ex.ToString()
                });
            }
        }

    }
}