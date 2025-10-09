using Microsoft.EntityFrameworkCore;
using MicroMercado.Data;
using MicroMercado.DTOs.Sales;
using MicroMercado.Models;
using System.Linq.Expressions;

namespace MicroMercado.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ApplicationDbContext context,
            ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Projection expression to keep Select logic in one place
        private static readonly Expression<Func<Product, ProductSearchDTO>> ProjectToDto = p => new ProductSearchDTO
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Brand = p.Brand,
            Price = p.Price,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category!.Name
        };

        // Build predicate for search to encapsulate the complex condition
        private static Expression<Func<Product, bool>> BuildSearchPredicate(string normalizedSearch)
        {
            return p => p.Status == 1 &&
                        p.Category != null &&
                        p.Category.Status == 1 &&
                        (p.Name.ToLower().Contains(normalizedSearch) ||
                         p.Description.ToLower().Contains(normalizedSearch) ||
                         p.Brand.ToLower().Contains(normalizedSearch) ||
                         p.Id.ToString().Contains(normalizedSearch) ||
                         p.Category.Name.ToLower().Contains(normalizedSearch));
        }

        public async Task<IEnumerable<ProductSearchDTO>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Enumerable.Empty<ProductSearchDTO>();
                }

                var normalizedSearch = searchTerm.Trim().ToLower();

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(BuildSearchPredicate(normalizedSearch))
                    .Select(ProjectToDto)
                    .OrderBy(p => p.Name)
                    .Take(20)
                    .ToListAsync();

                _logger.LogInformation(
                    "Búsqueda de productos con término '{SearchTerm}' retornó {Count} resultados",
                    searchTerm, products.Count);

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al buscar productos con término: {SearchTerm}",
                    searchTerm);
                throw;
            }
        }

        public async Task<ProductSearchDTO?> GetProductByIdAsync(short productId)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Id == productId && p.Status == 1)
                    .Select(ProjectToDto)
                    .FirstOrDefaultAsync();

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al obtener producto con ID: {ProductId}",
                    productId);
                throw;
            }
        }

        public async Task<bool> HasStockAsync(short productId, short quantity)
        {
            try
            {
                var stock = await _context.Products
                    .Where(p => p.Id == productId && p.Status == 1)
                    .Select(p => (short?)p.Stock)
                    .FirstOrDefaultAsync();

                return stock.HasValue && stock.Value >= quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al verificar stock del producto {ProductId}",
                    productId);
                throw;
            }
        }
    }
}