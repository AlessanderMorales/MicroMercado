using Microsoft.EntityFrameworkCore;
using MicroMercado.Data;
using MicroMercado.DTOs.Sales;
using MicroMercado.Models;

namespace MicroMercado.Services.sales;

public class SaleService : ISaleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        ApplicationDbContext context,
        ILogger<SaleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>> CreateSaleAsync(SaleDTO.CreateSaleDTO saleDTO)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            //Validar que haya items
            if (saleDTO.Items == null || !saleDTO.Items.Any())
            {
                return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>
                {
                    Success = false,
                    Message = "No hay productos en la venta"
                };
            }

            //Validar stock
            var stockValidation = await ValidateStockAsync(saleDTO.Items);
            if (!stockValidation.Success)
            {
                return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>
                {
                    Success = false,
                    Message = stockValidation.Message,
                    Errors = stockValidation.Errors
                };
            }

            //Crear la venta
            var sale = new Sale
            {
                UserId = 0,
                SaleDate = DateTime.Now,
                TotalAmount = saleDTO.TotalAmount,
                ClientId = saleDTO.ClientId
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            //Crear los items de venta
            foreach (var itemDTO in saleDTO.Items)
            {
                var saleItem = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = itemDTO.ProductId,
                    Quantity = itemDTO.Quantity,
                    Price = itemDTO.Price
                };

                _context.SaleItems.Add(saleItem);
            }

            await _context.SaveChangesAsync();

            //Actualizar stock de productos
            var stockUpdated = await UpdateProductStockAsync(saleDTO.Items);
            if (!stockUpdated)
            {
                await transaction.RollbackAsync();
                return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>
                {
                    Success = false,
                    Message = "Error al actualizar el stock de productos"
                };
            }

            //Confirmar transacción
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Venta creada exitosamente. ID: {SaleId}, Total: {Total}, Items: {ItemCount}",
                sale.Id, sale.TotalAmount, saleDTO.Items.Count);


            return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>
            {
                Success = true,
                Message = "Venta registrada exitosamente",
                Data = new SaleDTO.SaleResponseDTO
                {
                    SaleId = sale.Id,
                    SaleDate = sale.SaleDate,
                    TotalAmount = sale.TotalAmount,
                    CashReceived = saleDTO.CashReceived,
                    Change = saleDTO.Change,
                    ItemsCount = saleDTO.Items.Count
                }
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al crear venta");

            return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>
            {
                Success = false,
                Message = "Error al procesar la venta",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<SaleDTO.OperationResultDTO<bool>> ValidateStockAsync(List<SaleDTO.SaleItemDTO> items)
    {
        try
        {
            var errors = new List<string>();

            foreach (var item in items)
            {
                var product = await _context.Products
                    .Where(p => p.Id == item.ProductId && p.Status == 1)
                    .Select(p => new { p.Name, p.Stock })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    errors.Add($"Producto con ID {item.ProductId} no encontrado o inactivo");
                    continue;
                }

                if (product.Stock < item.Quantity)
                {
                    errors.Add($"Stock insuficiente para '{product.Name}'. Disponible: {product.Stock}, Solicitado: {item.Quantity}");
                }
            }

            if (errors.Any())
            {
                return new SaleDTO.OperationResultDTO<bool>
                {
                    Success = false,
                    Message = "Validación de stock fallida",
                    Errors = errors
                };
            }

            return new SaleDTO.OperationResultDTO<bool>
            {
                Success = true,
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar stock");
            return new SaleDTO.OperationResultDTO<bool>
            {
                Success = false,
                Message = "Error al validar stock",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<bool> UpdateProductStockAsync(List<SaleDTO.SaleItemDTO> items)
    {
        try
        {
            foreach (var item in items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar stock de productos: {Message}", ex.InnerException?.Message ?? ex.Message);
            return false;
        }
    }
}