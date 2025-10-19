using FluentValidation;
using MicroMercado.Application.DTOs.Product;

namespace MicroMercado.Application.Validators.Product;

public class UpdateProductValidator : AbstractValidator<UpdateProductDTO>
{
    public UpdateProductValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThan((short)0)
            .WithMessage("ID inválido para actualizar el producto");

        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("El nombre del producto es obligatorio")
            .MaximumLength(50)
            .WithMessage("El nombre no puede exceder los 50 caracteres")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s.,#/()\-]+$")
            .WithMessage("El nombre contiene caracteres inválidos");

        RuleFor(p => p.Description)
            .MaximumLength(50)
            .WithMessage("La descripción no puede exceder los 50 caracteres")
            .When(p => !string.IsNullOrWhiteSpace(p.Description));

        RuleFor(p => p.Brand)
            .NotEmpty()
            .WithMessage("La marca es obligatoria")
            .MaximumLength(20)
            .WithMessage("La marca no puede exceder los 20 caracteres")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s&.\-']+$")
            .WithMessage("La marca contiene caracteres inválidos");

        RuleFor(p => p.Price)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(9999.99m)
            .WithMessage("El precio no puede exceder 9999.99")
            .Must(BeAValidDecimal)
            .WithMessage("El precio solo puede tener hasta 2 decimales");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo((short)0)
            .WithMessage("El stock no puede ser negativo")
            .LessThanOrEqualTo((short)9999)
            .WithMessage("El stock no puede exceder 9999");

        RuleFor(p => p.CategoryId)
            .GreaterThan((byte)0)
            .WithMessage("Debe seleccionar una categoría válida");

        RuleFor(p => p.Status)
            .Must(s => s == 0 || s == 1)
            .WithMessage("El estado debe ser 0 (inactivo) o 1 (activo)");
    }
    
    private bool BeAValidDecimal(decimal price)
    {
        return price == Math.Round(price, 2);
    }
}