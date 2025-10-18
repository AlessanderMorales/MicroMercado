namespace MicroMercado.Domain.Models;

public class Client
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } 
    public string TaxDocument { get; set; } = string.Empty;
    public byte Status { get; set; } = 1;
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}