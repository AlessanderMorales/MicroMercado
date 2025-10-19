namespace MicroMercado.Application.DTOs.Category;

public class UpdateCategoryDTO
{
    public byte Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte Status { get; set; }
}