namespace MicroMercado.Models;

public class SaleItem
{
    public int SaleId { get; set; }
    public short ProductId { get; set; }
    
    public short Quantity { get; set; }
    public decimal Price { get; set; }
    
    public Sale? Sale { get; set; }
    
    public Product? Product { get; set; }
}