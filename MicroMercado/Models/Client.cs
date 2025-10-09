namespace MicroMercado.Models;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TaxDocument { get; set; } = string.Empty;
    public byte Status { get; set; } = 1;
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}