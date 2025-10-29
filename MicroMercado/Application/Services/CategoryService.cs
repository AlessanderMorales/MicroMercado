using FluentValidation;
using MicroMercado.Application.DTOs.Category;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroMercado.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<CreateCategoryDTO> _createCategoryValidator;
    private readonly IValidator<UpdateCategoryDTO> _updateCategoryValidator;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ApplicationDbContext context,
        IValidator<CreateCategoryDTO> createCategoryValidator,
        IValidator<UpdateCategoryDTO> updateCategoryValidator,
        ILogger<CategoryService> logger)
    {
        _context = context;
        _createCategoryValidator = createCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
        _logger = logger;
    }

    private static CategoryDTO MapToCategoryDTO(Category category)
    {
        return new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Status = category.Status,
            LastUpdate = DateTime.SpecifyKind(category.LastUpdate, DateTimeKind.Unspecified)
        };
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.Status == 1) // mostrar solo activos por defecto, igual que en Cliente
            .Select(c => new CategoryDTO {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status,
                LastUpdate = DateTime.SpecifyKind(c.LastUpdate, DateTimeKind.Unspecified)
            })
            .ToListAsync();
    }

    public async Task<CategoryDTO?> GetCategoryByIdAsync(byte id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;
        return MapToCategoryDTO(category);
    }

    public async Task<CategoryDTO?> CreateCategoryAsync(CreateCategoryDTO categoryDto)
    {
        var validationResult = await _createCategoryValidator.ValidateAsync(categoryDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation errors creating category: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return null;
        }

        var existingCategory = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == categoryDto.Name.Trim().ToLower());

        if (existingCategory)
        {
            _logger.LogWarning("Category with name {Name} already exists.", categoryDto.Name);
            return null;
        }

        var category = new Category
        {
            Name = categoryDto.Name.Trim(),
            Description = categoryDto.Description?.Trim(),
            Status = 1,
            LastUpdate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToCategoryDTO(category);
    }

    public async Task<CategoryDTO?> UpdateCategoryAsync(UpdateCategoryDTO categoryDto)
    {
        var validationResult = await _updateCategoryValidator.ValidateAsync(categoryDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation errors updating category: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return null;
        }

        var categoryToUpdate = await _context.Categories.FindAsync(categoryDto.Id);
        if (categoryToUpdate == null)
        {
            _logger.LogWarning("Category with ID {Id} not found for update.", categoryDto.Id);
            return null;
        }

        var existingCategory = await _context.Categories
            .AnyAsync(c => c.Id != categoryDto.Id && c.Name.ToLower() == categoryDto.Name.Trim().ToLower());

        if (existingCategory)
        {
            _logger.LogWarning("Another category with name {Name} already exists.", categoryDto.Name);
            return null;
        }

        categoryToUpdate.Name = categoryDto.Name.Trim();
        categoryToUpdate.Description = categoryDto.Description?.Trim();
        categoryToUpdate.Status = categoryDto.Status;
        categoryToUpdate.LastUpdate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        await _context.SaveChangesAsync();

        return MapToCategoryDTO(categoryToUpdate);
    }

    public async Task<bool> DeleteCategoryAsync(byte id)
    {
        var categoryToDelete = await _context.Categories.FindAsync(id);
        if (categoryToDelete == null)
        {
            _logger.LogWarning("Attempt to delete category ID {Id} but not found.", id);
            return false;
        }

        categoryToDelete.Status = 0;
        categoryToDelete.LastUpdate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Category with ID {Id} logically deleted (Status set to 0).", id);
        return true;
    }
}
