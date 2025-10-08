using MicroMercado.DTOs.Sales;

namespace MicroMercado.Services;

public interface IProductService
{
    Task<IEnumerable<ProductSearchDTO>> SearchProductsAsync(string searchTerm);

    Task<ProductSearchDTO?> GetProductByIdAsync(short productId);

    Task<bool> HasStockAsync(short productId, short quantity);
}