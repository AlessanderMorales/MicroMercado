using System.Linq.Expressions;
using FluentValidation;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.DTOs.Sales;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroMercado.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IValidator<CreateProductDTO> _createProductValidator;
        private readonly IValidator<UpdateProductDTO> _updateProductValidator;

        public ProductService(
            ApplicationDbContext context,
            ILogger<ProductService> logger,
            IValidator<CreateProductDTO> createProductValidator,
            IValidator<UpdateProductDTO> updateProductValidator)
        {
            _context = context;
            _logger = logger;
            _createProductValidator = createProductValidator;
            _updateProductValidator = updateProductValidator;
        }

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

        public async Task<IEnumerable<ProductSearchDTO>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<ProductSearchDTO>();

            try
            {
                var normalizedSearch = searchTerm.Trim().ToLower();
                //var predicate = BuildSimplifiedPredicate(normalizedSearch);

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Status == 1 && 
                                p.Category != null && 
                                p.Category.Status == 1 &&
                                (p.Name.ToLower().Contains(normalizedSearch) ||
                                 p.Description.ToLower().Contains(normalizedSearch) ||
                                 p.Brand.ToLower().Contains(normalizedSearch) ||
                                 p.Category.Name.ToLower().Contains(normalizedSearch)))
                    .Select(p => new ProductSearchDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Brand = p.Brand,
                        Price = p.Price,
                        Stock = p.Stock,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category!.Name
                    })
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
        
        
        private static ProductDTO MapToProductDTO(Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                Status = product.Status,
                LastUpdate = product.LastUpdate
            };
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Select(p => MapToProductDTO(p))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los productos");
                throw;
            }
        }

        public async Task<ProductDTO?> GetProductDetailsByIdAsync(short id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                return product != null ? MapToProductDTO(product) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del producto {ProductId}", id);
                throw;
            }
        }

        public async Task<Product?> CreateProductAsync(CreateProductDTO productDto)
        {
            var validationResult = await _createProductValidator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation errors creating product: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return null;
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == productDto.CategoryId && c.Status == 1);

            if (!categoryExists)
            {
                _logger.LogWarning("Category {CategoryId} not found or inactive", productDto.CategoryId);
                return null;
            }
            
            var existingProduct = await _context.Products
                .AnyAsync(p => p.Name.ToLower() == productDto.Name.Trim().ToLower());

            if (existingProduct)
            {
                _logger.LogWarning("Product with name {Name} already exists", productDto.Name);
                return null;
            }

            var product = new Product
            {
                Name = productDto.Name.Trim(),
                Description = productDto.Description?.Trim() ?? string.Empty,
                Brand = productDto.Brand.Trim(),
                Price = productDto.Price,
                Stock = productDto.Stock,
                CategoryId = productDto.CategoryId,
                Status = 1,
                LastUpdate = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Product {ProductId} created successfully: {ProductName}", 
                product.Id, product.Name);
            return product;
        }

        public async Task<Product?> UpdateProductAsync(UpdateProductDTO productDto)
        {
            var validationResult = await _updateProductValidator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation errors updating product: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return null;
            }

            var productToUpdate = await _context.Products.FindAsync(productDto.Id);
            if (productToUpdate == null)
            {
                _logger.LogWarning("Product {ProductId} not found", productDto.Id);
                return null;
            }
            
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == productDto.CategoryId && c.Status == 1);

            if (!categoryExists)
            {
                _logger.LogWarning("Category {CategoryId} not found or inactive", productDto.CategoryId);
                return null;
            }
            
            var existingProduct = await _context.Products
                .AnyAsync(p => p.Id != productDto.Id && 
                              p.Name.ToLower() == productDto.Name.Trim().ToLower());

            if (existingProduct)
            {
                _logger.LogWarning("Another product with name {Name} already exists", productDto.Name);
                return null;
            }

            productToUpdate.Name = productDto.Name.Trim();
            productToUpdate.Description = productDto.Description?.Trim() ?? string.Empty;
            productToUpdate.Brand = productDto.Brand.Trim();
            productToUpdate.Price = productDto.Price;
            productToUpdate.Stock = productDto.Stock;
            productToUpdate.CategoryId = productDto.CategoryId;
            productToUpdate.Status = productDto.Status;
            productToUpdate.LastUpdate = DateTime.Now;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Product {ProductId} updated successfully: {ProductName}", 
                productDto.Id, productToUpdate.Name);
            return productToUpdate;
        }

        public async Task<bool> DeleteProductAsync(short id)
        {
            var productToDelete = await _context.Products.FindAsync(id);
            if (productToDelete == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return false;
            }
            
            productToDelete.Status = 0;
            _context.Products.Update(productToDelete);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Product {ProductId} deleted (soft delete): {ProductName}", 
                id, productToDelete.Name);
            return true;
        }
    }
}
