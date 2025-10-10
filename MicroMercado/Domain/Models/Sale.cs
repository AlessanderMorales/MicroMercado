namespace MicroMercado.Domain.Models;

public class Sale
{
    public int Id { get; set; }
    public short UserId { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public int? ClientId { get; set; }
    
    public Client? Client { get; set; }
    
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}