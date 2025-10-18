namespace MicroMercado.Application.DTOs.Client;

public class CreateClientDTO
{
    public string BusinessName { get; set; } = string.Empty;
    public string? Email { get; set; }  
    public string? Address { get; set; }
    public string TaxDocument { get; set; } = string.Empty;
}