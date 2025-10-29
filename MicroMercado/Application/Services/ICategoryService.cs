using MicroMercado.Application.DTOs.Category;
using MicroMercado.Domain.Models;

namespace MicroMercado.Application.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
    Task<CategoryDTO?> GetCategoryByIdAsync(byte id);
    Task<CategoryDTO?> CreateCategoryAsync(CreateCategoryDTO categoryDto);
    Task<CategoryDTO?> UpdateCategoryAsync(UpdateCategoryDTO categoryDto);
    Task<bool> DeleteCategoryAsync(byte id);
}
