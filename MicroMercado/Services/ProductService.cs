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

        // =========================
        // Public Methods
        // =========================

        public async Task<IEnumerable<ProductSearchDTO>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<ProductSearchDTO>();

            try
            {
                var normalizedSearch = searchTerm.Trim().ToLower();
                var predicate = BuildSimplifiedPredicate(normalizedSearch);

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(predicate)
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

        // =========================
        // Private Helpers
        // =========================

        private static Expression<Func<Product, bool>> BuildSimplifiedPredicate(string term)
        {
            return p => p.Status == 1
                        && p.Category != null
                        && p.Category.Status == 1
                        && (ContainsTerm(p.Name, term)
                            || ContainsTerm(p.Description, term)
                            || ContainsTerm(p.Brand, term)
                            || ContainsTerm(p.Category.Name, term));
        }

        private static bool ContainsTerm(string? field, string term)
        {
            return !string.IsNullOrEmpty(field) && field.ToLower().Contains(term);
        }
    }
}
