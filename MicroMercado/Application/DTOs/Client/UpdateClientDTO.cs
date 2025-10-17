
namespace MicroMercado.Application.DTOs.Client;

public class UpdateClientDTO
{
    public int Id { get; set; } 
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxDocument { get; set; } = string.Empty;
    public byte Status { get; set; }
}