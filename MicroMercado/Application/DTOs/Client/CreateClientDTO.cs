namespace MicroMercado.Application.DTOs.Client;

public class CreateClientDTO
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TaxDocument { get; set; } = string.Empty;
}