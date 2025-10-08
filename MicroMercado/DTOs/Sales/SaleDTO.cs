namespace MicroMercado.DTOs.Sales;

public class SaleDTO
{
    public class CreateSaleDTO
    {
        public int ClientId { get; set; }
        public byte PaymentType { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashReceived { get; set; }
        public decimal Change { get; set; }
        public List<SaleItemDTO> Items { get; set; } = new();
    }
    
    public class SaleItemDTO
    {
        public short ProductId { get; set; }
        public short Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }


    public class SaleResponseDTO
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashReceived { get; set; }
        public decimal Change { get; set; }
        public int ItemsCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }


    public class OperationResultDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}