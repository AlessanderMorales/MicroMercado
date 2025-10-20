using System.Text.Json;
using MicroMercado.Application.DTOs.Client;
using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace MicroMercado.Presentation.Pages
{
    public class SalesModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly IClientService _clientService; 
        private readonly ILogger<SalesModel> _logger;

        public SalesModel(
            IProductService productService,
            ISaleService saleService, 
            IClientService clientService, 
            ILogger<SalesModel> logger)
        {
            _productService = productService;
            _saleService = saleService;
            _clientService = clientService; 
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string? ClientTaxDocument { get; set; } 
        public ClientDTO? FoundClient { get; set; } 


        public void OnGet()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewData["SuccessMessage"] = TempData["SuccessMessage"];
            }
            
            if (TempData["ClientTaxDocument"] != null)
            {
                ClientTaxDocument = TempData["ClientTaxDocument"]?.ToString();
            }
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

        public async Task<IActionResult> OnPostSearchClientByTaxDocumentAsync()
        {
            if (string.IsNullOrWhiteSpace(ClientTaxDocument))
            {
                return new JsonResult(new { success = false, message = "El NIT/CI no puede estar vacío." });
            }

            try
            {
                var client = await _clientService.GetClientByTaxDocumentAsync(ClientTaxDocument);

                if (client == null)
                {
                    return new JsonResult(new { success = false, message = "Cliente no encontrado." });
                }
                FoundClient = new ClientDTO
                {
                    Id = client.Id,
                    BusinessName = client.BusinessName,
                    Email = client.Email,
                    Address = client.Address,
                    TaxDocument = client.TaxDocument,
                    Status = (byte)client.Status,
                    LastUpdate = client.LastUpdate
                };

                return new JsonResult(new { success = true, client = FoundClient });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar cliente por documento tributario: {ClientTaxDocument}", ClientTaxDocument);
                return new JsonResult(new { success = false, message = "Error interno al buscar cliente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostConfirmSaleAsync()
        {
            try
            {
                _logger.LogInformation("=== INICIO OnPostConfirmSaleAsync ===");
                _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
                _logger.LogInformation("Content-Length: {ContentLength}", Request.ContentLength);

                string rawBody = "";
                using (var reader = new StreamReader(Request.Body))
                {
                    rawBody = await reader.ReadToEndAsync();
                }
                _logger.LogInformation("recibido: {RawBody}", rawBody);

                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    _logger.LogWarning("Body está vacío");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se recibió ningún dato en el body"
                    });
                }

                SaleDTO.CreateSaleDTO? saleDTO = null;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    saleDTO = JsonSerializer.Deserialize<SaleDTO.CreateSaleDTO>(rawBody, options);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Error al deserializar JSON");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "JSON malformado: " + jsonEx.Message
                    });
                }

                if (saleDTO == null)
                {
                    _logger.LogWarning("saleDTO es null después de deserializar");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se pudo procesar el JSON recibido"
                    });
                }

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
                    _logger.LogWarning("No hay items en la venta");
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No hay productos para vender"
                    });
                }

                var calculatedTotal = saleDTO.Items.Sum(i => i.Total);
                _logger.LogInformation("Total calculado: {Calculated}, Total recibido: {Received}",
                    calculatedTotal, saleDTO.TotalAmount);

                if (Math.Abs(calculatedTotal - saleDTO.TotalAmount) > 0.01m)
                {
                    _logger.LogWarning("⚠️ Total no coincide exactamente pero continúa");
                }

                _logger.LogInformation("Llamando a _saleService.CreateSaleAsync...");
                var result = await _saleService.CreateSaleAsync(saleDTO);
                _logger.LogInformation("Respuesta del servicio: Success={Success}, Message={Message}",
                    result.Success, result.Message);

                if (result.Success)
                {
                    _logger.LogInformation("✅ Venta creada exitosamente. SaleId: {SaleId}");
                    return new JsonResult(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data
                    });
                }

                _logger.LogWarning("Error al crear venta: {Message}", result.Message);
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