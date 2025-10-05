namespace MicroMercado.DTOs;

public class UpdateClientDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TaxDocument { get; set; } = string.Empty;
    public byte Status { get; set; }
}