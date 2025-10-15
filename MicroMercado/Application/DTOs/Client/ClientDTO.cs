namespace MicroMercado.Application.DTOs.Client;

public class ClientDTO
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxDocument { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusText => Status == 1 ? "Activo" : "Inactivo";
    public DateTime LastUpdate { get; set; }
}