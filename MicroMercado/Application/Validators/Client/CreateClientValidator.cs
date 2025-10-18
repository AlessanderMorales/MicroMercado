using FluentValidation;
using MicroMercado.Application.DTOs.Client;

namespace MicroMercado.Application.Validators.Client;

public class CreateClientValidator : AbstractValidator<CreateClientDTO>
{
    public CreateClientValidator()
    {
        RuleFor(c => c.BusinessName)
            .NotEmpty().WithMessage("El nombre o razón social es obligatorio")
            .MaximumLength(150).WithMessage("El nombre o razón social no puede tener más de 150 caracteres")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,#/-]+$")
            .WithMessage("El nombre o razón social contiene caracteres inválidos");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("El email es obligatorio") 
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(100).WithMessage("El email no puede tener más de 100 caracteres");


        RuleFor(c => c.Address)
            .MaximumLength(150).WithMessage("La dirección no puede tener más de 150 caracteres")
            .When(c => !string.IsNullOrWhiteSpace(c.Address));

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