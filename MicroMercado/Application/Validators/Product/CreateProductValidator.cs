using FluentValidation;
using MicroMercado.Application.DTOs.Product;

namespace MicroMercado.Application.Validators.Product;

public class CreateProductValidator : AbstractValidator<CreateProductDTO>
{
    public CreateProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio")
            .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,#/-]+$")
            .WithMessage("El nombre contiene caracteres inválidos");

        RuleFor(p => p.Description)
            .MaximumLength(50).WithMessage("La descripción no puede exceder los 50 caracteres")
            .When(p => !string.IsNullOrWhiteSpace(p.Description));

        RuleFor(p => p.Brand)
            .NotEmpty().WithMessage("La marca es obligatoria")
            .MaximumLength(20).WithMessage("La marca no puede exceder los 20 caracteres")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s&.-]+$")
            .WithMessage("La marca contiene caracteres inválidos");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
            .LessThan(10000).WithMessage("El precio no puede exceder 9999.99");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo((short)0).WithMessage("El stock no puede ser negativo")
            .LessThan((short)10000).WithMessage("El stock no puede exceder 9999");

        RuleFor(p => p.CategoryId)
            .GreaterThan((byte)0).WithMessage("Debe seleccionar una categoría válida");
    }
}