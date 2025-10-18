namespace MicroMercado.Application.DTOs.Category;

public class CategoryDTO
{
    public byte Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusText => Status == 1 ? "Activo" : "Inactivo";
    public DateTime LastUpdate { get; set; }
}