namespace MicroMercado.Application.DTOs.Client;

public class CreateClientDTO
{
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty; 
    public string TaxDocument { get; set; } = string.Empty;
}