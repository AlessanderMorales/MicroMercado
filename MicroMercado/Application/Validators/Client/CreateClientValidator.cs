using FluentValidation;
using MicroMercado.Application.DTOs.Client;

namespace MicroMercado.Application.Validators.Client
{
    public class CreateClientValidator : AbstractValidator<CreateClientDTO>
    {
        public CreateClientValidator()
        {
            RuleFor(c => c.BusinessName)
                .NotEmpty().WithMessage("El nombre o razón social es obligatorio")
                .MinimumLength(3).WithMessage("El nombre o razón social debe tener al menos 3 caracteres")
                .MaximumLength(100).WithMessage("El nombre o razón social no puede tener más de 100 caracteres")
                .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,#/-]+$")
                .WithMessage("El nombre o razón social contiene caracteres inválidos");
            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("El email es obligatorio")
                .EmailAddress().WithMessage("El formato del email no es válido")
                .MaximumLength(150).WithMessage("El email no puede tener más de 150 caracteres");
            RuleFor(c => c.Address)
                .MinimumLength(5).WithMessage("La dirección debe tener al menos 5 caracteres")
                .MaximumLength(150).WithMessage("La dirección no puede tener más de 150 caracteres")
                .When(c => !string.IsNullOrWhiteSpace(c.Address));
            RuleFor(c => c.TaxDocument)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El documento es obligatorio")
                .Matches(@"^[0-9]+$").WithMessage("El documento solo puede contener números")
                .Must(BeValidTaxDocument).WithMessage("El documento debe tener entre 6 y 9 dígitos");
        }
        private bool BeValidTaxDocument(string taxDocument)
        {
            if (string.IsNullOrWhiteSpace(taxDocument))
                return false;

            var cleanDocument = taxDocument.Replace("-", "");
            return cleanDocument.Length >= 6 && cleanDocument.Length < 9;
        }
    }
}
