using FluentValidation;
using MicroMercado.Application.DTOs.Client;

namespace MicroMercado.Application.Validators.Client;

public class UpdateClientValidator : AbstractValidator<UpdateClientDTO>
{
    public UpdateClientValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0).WithMessage("ID inválido");
        
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
            .WithMessage("El documento solo puede contener números");
        
        RuleFor(c => c.Status)
            .Must(s => s == 0 || s == 1)
            .WithMessage("El estado debe ser 0 (inactivo) o 1 (activo)");
    }
}