using FluentValidation;
using MicroMercado.Application.DTOs.Client;

namespace MicroMercado.Application.Validators.Client;

public class CreateClientValidator : AbstractValidator<CreateClientDTO>
{
    public CreateClientValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(20).WithMessage("El nombre no puede tener más de 20 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
            .WithMessage("El nombre solo puede contener letras");
        
        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio")
            .MaximumLength(20).WithMessage("El apellido no puede tener más de 20 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
            .WithMessage("El apellido solo puede contener letras");
        
        RuleFor(c => c.TaxDocument)
            .NotEmpty().WithMessage("El documento es obligatorio")
            .MaximumLength(20).WithMessage("El documento no puede tener más de 20 caracteres")
            .Matches(@"^[0-9]+$")
            .WithMessage("El documento solo puede contener números")
            .Must(BeValidTaxDocument)
            .WithMessage("El formato del documento no es válido");
    }
    
    private bool BeValidTaxDocument(string taxDocument)
    {
        if (string.IsNullOrWhiteSpace(taxDocument))
            return false;
        
        var cleanDocument = taxDocument.Replace("-", "");

        return cleanDocument.Length >= 6 && cleanDocument.Length <= 15;
    }
}