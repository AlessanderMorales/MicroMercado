using MicroMercado.Application.DTOs.Category;
using MicroMercado.Domain.Models;

namespace MicroMercado.Application.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
    Task<CategoryDTO?> GetCategoryByIdAsync(byte id);
    Task<Category?> CreateCategoryAsync(CreateCategoryDTO categoryDto);
    Task<Category?> UpdateCategoryAsync(UpdateCategoryDTO categoryDto);
    Task<bool> DeleteCategoryAsync(byte id);
}