namespace MicroMercado.Application.DTOs.Product;

public class ProductDTO
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public short Stock { get; set; }
    public byte CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusText => Status == 1 ? "Activo" : "Inactivo";
    public DateTime LastUpdate { get; set; }
}