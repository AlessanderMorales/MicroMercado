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

        // --- Projection expression ---
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

        // --- Predicates encapsulated to reduce complexity ---
        private static class ProductPredicates
        {
            public static Expression<Func<Product, bool>> IsActive() =>
                p => p.Status == 1 && p.Category != null && p.Category.Status == 1;

            public static Expression<Func<Product, bool>> MatchesSearch(string term) =>
                p => new[]
                {
                    p.Name.ToLower(),
                    p.Description.ToLower(),
                    p.Brand.ToLower(),
                    p.Id.ToString(),
                    p.Category!.Name.ToLower()
                }.Any(field => field.Contains(term));
        }

        // --- Combines two expressions dynamically ---
        private static Expression<Func<Product, bool>> BuildSearchPredicate(string normalizedSearch)
        {
            var active = ProductPredicates.IsActive();
            var match = ProductPredicates.MatchesSearch(normalizedSearch);
            return active.And(match);
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

    // --- Helper class to combine expressions with AND logic ---
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var param = Expression.Parameter(typeof(T));
            var combined = Expression.AndAlso(
                Expression.Invoke(left, param),
                Expression.Invoke(right, param)
            );
            return Expression.Lambda<Func<T, bool>>(combined, param);
        }
    }
}
