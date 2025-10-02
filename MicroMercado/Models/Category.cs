namespace MicroMercado.Models;

public class Category
{
    public byte Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Status { get; set; } = 1;
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
}