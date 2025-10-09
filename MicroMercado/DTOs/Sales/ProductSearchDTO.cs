namespace MicroMercado.DTOs.Sales;

public class ProductSearchDTO
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public short Stock { get; set; }
    public byte CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool HasStock => Stock > 0;
}