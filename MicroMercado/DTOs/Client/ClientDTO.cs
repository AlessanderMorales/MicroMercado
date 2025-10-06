namespace MicroMercado.DTOs;

public class ClientDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{Name} {LastName}";
    public string TaxDocument { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusText => Status == 1 ? "Activo" : "Inactivo";
    public DateTime LastUpdate { get; set; }
}