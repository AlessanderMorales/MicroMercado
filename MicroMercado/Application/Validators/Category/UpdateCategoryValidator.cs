using FluentValidation;
using MicroMercado.Application.DTOs.Category;

namespace MicroMercado.Application.Validators.Category;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDTO>
{
    public UpdateCategoryValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan((byte)0).WithMessage("ID inválido para actualizar la categoría.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(20).WithMessage("El nombre no puede exceder los 20 caracteres.")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre contiene caracteres inválidos.");

        RuleFor(c => c.Description)
            .MaximumLength(80).WithMessage("La descripción no puede exceder los 80 caracteres.")
            .When(c => !string.IsNullOrWhiteSpace(c.Description));

        RuleFor(c => c.Status)
            .Must(s => s == 0 || s == 1)
            .WithMessage("El estado debe ser 0 (inactivo) o 1 (activo)");
    }
}