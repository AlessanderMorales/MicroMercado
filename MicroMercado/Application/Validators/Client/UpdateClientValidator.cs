using FluentValidation;
using MicroMercado.Application.DTOs.Client;

namespace MicroMercado.Application.Validators.Client;

public class UpdateClientValidator : AbstractValidator<UpdateClientDTO>
{
    public UpdateClientValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0).WithMessage("ID inválido para actualizar el cliente.");

        RuleFor(c => c.BusinessName)
            .NotEmpty().WithMessage("El nombre o razón social es obligatorio")
            .MaximumLength(150).WithMessage("El nombre o razón social no puede tener más de 150 caracteres")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,#/-]+$")
            .WithMessage("El nombre o razón social contiene caracteres inválidos");


        RuleFor(c => c.Email)
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(100).WithMessage("El email no puede tener más de 100 caracteres")
            .When(c => !string.IsNullOrWhiteSpace(c.Email)); 

        RuleFor(c => c.Address)
            .MaximumLength(150).WithMessage("La dirección no puede tener más de 150 caracteres")
            .When(c => !string.IsNullOrWhiteSpace(c.Address));

        RuleFor(c => c.TaxDocument)
            .NotEmpty().WithMessage("El documento es obligatorio")
            .MaximumLength(20).WithMessage("El documento no puede tener más de 20 caracteres")
            .Matches(@"^[0-9]+$")
            .WithMessage("El documento solo puede contener números");

        RuleFor(c => c.Status)
            .Must(s => s == 0 || s == 1)
            .WithMessage("El estado debe ser 0 (inactivo) o 1 (activo)");
    }
}