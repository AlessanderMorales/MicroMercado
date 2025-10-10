namespace MicroMercado.Application.DTOs.Sales;

public class CartItemDTO
{
    public short ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public short Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
    public bool AppliesWeight { get; set; }
}