using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Domain.Models;

namespace MicroMercado.Application.Services;

public interface IProductService
{
    Task<IEnumerable<ProductSearchDTO>> SearchProductsAsync(string searchTerm);

    Task<ProductSearchDTO?> GetProductByIdAsync(short productId);

    Task<bool> HasStockAsync(short productId, short quantity);
    
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<ProductDTO?> GetProductDetailsByIdAsync(short id);
    Task<Product?> CreateProductAsync(CreateProductDTO productDto);
    Task<Product?> UpdateProductAsync(UpdateProductDTO productDto);
    Task<bool> DeleteProductAsync(short id);
}