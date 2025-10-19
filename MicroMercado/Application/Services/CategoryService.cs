using FluentValidation;
using MicroMercado.Application.DTOs.Category;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

    public CategoryService(
        ApplicationDbContext context,
        IValidator<CreateCategoryDTO> createCategoryValidator,
        IValidator<UpdateCategoryDTO> updateCategoryValidator)
    {
        _context = context;
        _createCategoryValidator = createCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
    }

    private CategoryDTO MapToCategoryDTO(Category category)
    {
        return new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Status = category.Status,
            LastUpdate = category.LastUpdate
        };
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Select(c => MapToCategoryDTO(c))
            .ToListAsync();
    }

    public async Task<CategoryDTO?> GetCategoryByIdAsync(byte id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category != null ? MapToCategoryDTO(category) : null;
    }

    public async Task<Category?> CreateCategoryAsync(CreateCategoryDTO categoryDto)
    {
        var validationResult = await _createCategoryValidator.ValidateAsync(categoryDto);
        if (!validationResult.IsValid)
        {
            Console.WriteLine($"Validation errors creating category: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
            return null;
        }

        var existingCategory = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == categoryDto.Name.Trim().ToLower());

        if (existingCategory)
        {
            Console.WriteLine($"Category with name {categoryDto.Name} already exists.");
            return null;
        }

        var category = new Category
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            Status = 1,
            LastUpdate = DateTime.Now
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(UpdateCategoryDTO categoryDto)
    {
        var validationResult = await _updateCategoryValidator.ValidateAsync(categoryDto);
        if (!validationResult.IsValid)
        {
            Console.WriteLine($"Validation errors updating category: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
            return null;
        }

        var categoryToUpdate = await _context.Categories.FindAsync(categoryDto.Id);
        if (categoryToUpdate == null)
        {
            return null;
        }

        var existingCategory = await _context.Categories
            .AnyAsync(c => c.Id != categoryDto.Id && c.Name.ToLower() == categoryDto.Name.Trim().ToLower());

        if (existingCategory)
        {
            Console.WriteLine($"Another category with name {categoryDto.Name} already exists.");
            return null;
        }

        categoryToUpdate.Name = categoryDto.Name;
        categoryToUpdate.Description = categoryDto.Description;
        categoryToUpdate.Status = categoryDto.Status;
        categoryToUpdate.LastUpdate = DateTime.Now;

        await _context.SaveChangesAsync();
        return categoryToUpdate;
    }

    public async Task<bool> DeleteCategoryAsync(byte id)
    {
        var categoryToDelete = await _context.Categories.FindAsync(id);
        if (categoryToDelete == null)
        {
            return false;
        }

        categoryToDelete.Status = 0;
        _context.Categories.Update(categoryToDelete);

        await _context.SaveChangesAsync();
        return true;
    }
}