namespace MicroMercado.Models;

public class Product
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public short Stock { get; set; }
    public byte CategoryId { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    
    public Category? Category { get; set; }
}