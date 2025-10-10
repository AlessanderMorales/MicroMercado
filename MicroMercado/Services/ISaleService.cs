using MicroMercado.DTOs.Sales;

namespace MicroMercado.Services;

public interface ISaleService
{
        Task<SaleDTO.OperationResultDTO<SaleDTO.SaleResponseDTO>> CreateSaleAsync(SaleDTO.CreateSaleDTO saleDTO);

        Task<SaleDTO.OperationResultDTO<bool>> ValidateStockAsync(List<SaleDTO.SaleItemDTO> items);
        
        Task<bool> UpdateProductStockAsync(List<SaleDTO.SaleItemDTO> items);
    
}