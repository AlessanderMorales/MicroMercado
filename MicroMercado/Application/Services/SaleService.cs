using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroMercado.Application.Services;

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
            var validationResult = await ValidateSaleRequestAsync(saleDTO);
            if (!validationResult.Success)
                return validationResult;

            var sale = await ProcessSaleAsync(saleDTO);

            await transaction.CommitAsync();

            return CreateSuccessResponse(sale, saleDTO);
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


    private async Task<SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>> ValidateSaleRequestAsync(SaleDTO.CreateSaleDTO saleDTO)
    {
        var itemsValidation = ValidateItems(saleDTO.Items);
        if (!itemsValidation.Success)
            return itemsValidation;


        if (saleDTO.ClientId.HasValue)
        {
            var clientExists = await _context.Clients
                .AnyAsync(c => c.Id == saleDTO.ClientId.Value && c.Status == 1);

            if (!clientExists)
            {
                throw new InvalidOperationException($"Cliente con ID {saleDTO.ClientId.Value} no encontrado o inactivo");
            }
        }

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

        return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO> { Success = true };
    }

    private SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO> ValidateItems(List<SaleDTO.SaleItemDTO> items)
    {
        if (items == null || !items.Any())
        {
            return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>
            {
                Success = false,
                Message = "No hay productos en la venta"
            };
        }

        return new SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO> { Success = true };
    }

    private async Task<Sale> ProcessSaleAsync(SaleDTO.CreateSaleDTO saleDTO)
    {
        var sale = CreateSaleEntity(saleDTO);
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        await CreateSaleItemsAsync(sale.Id, saleDTO.Items);
        await UpdateProductStockAsync(saleDTO.Items);

        LogSaleCreation(sale, saleDTO.Items.Count);

        return sale;
    }

    private Sale CreateSaleEntity(SaleDTO.CreateSaleDTO saleDTO)
    {
        return new Sale
        {
            UserId = 0,
            SaleDate = DateTime.Now,
            TotalAmount = saleDTO.TotalAmount,
            ClientId = saleDTO.ClientId
        };
    }

    private async Task CreateSaleItemsAsync(int saleId, List<SaleDTO.SaleItemDTO> items)
    {
        var saleItems = items.Select(itemDTO => new SaleItem
        {
            SaleId = saleId,
            ProductId = itemDTO.ProductId,
            Quantity = itemDTO.Quantity,
            Price = itemDTO.Price
        });

        _context.SaleItems.AddRange(saleItems);
        await _context.SaveChangesAsync();
    }

    private SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO> CreateSuccessResponse(Sale sale, SaleDTO.CreateSaleDTO saleDTO)
    {
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

    private void LogSaleCreation(Sale sale, int itemCount)
    {
        _logger.LogInformation(
            "Venta creada exitosamente. ID: {SaleId}, Total: {Total}, Items: {ItemCount}",
            sale.Id, sale.TotalAmount, itemCount);
    }

    public async Task<SaleDTO.OperationResultDTO<bool>> ValidateStockAsync(List<SaleDTO.SaleItemDTO> items)
    {
        try
        {
            var errors = await CollectStockValidationErrorsAsync(items);

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

    private async Task<List<string>> CollectStockValidationErrorsAsync(List<SaleDTO.SaleItemDTO> items)
    {
        var errors = new List<string>();

        foreach (var item in items)
        {
            var error = await ValidateSingleItemStockAsync(item);
            if (error != null)
                errors.Add(error);
        }

        return errors;
    }

    private async Task<string> ValidateSingleItemStockAsync(SaleDTO.SaleItemDTO item)
    {
        var product = await _context.Products
            .Where(p => p.Id == item.ProductId && p.Status == 1)
            .Select(p => new { p.Name, p.Stock })
            .FirstOrDefaultAsync();

        if (product == null)
            return $"Producto con ID {item.ProductId} no encontrado o inactivo";

        if (product.Stock < item.Quantity)
            return $"Stock insuficiente para '{product.Name}'. Disponible: {product.Stock}, Solicitado: {item.Quantity}";

        return null;
    }

    public async Task<bool> UpdateProductStockAsync(List<SaleDTO.SaleItemDTO> items)
    {
        try
        {
            await UpdateStockForItemsAsync(items);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar stock de productos: {Message}", ex.InnerException?.Message ?? ex.Message);
            return false;
        }
    }

    private async Task UpdateStockForItemsAsync(List<SaleDTO.SaleItemDTO> items)
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
    }
}