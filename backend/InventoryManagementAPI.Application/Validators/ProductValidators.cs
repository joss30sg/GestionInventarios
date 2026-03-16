using FluentValidation;
using InventoryManagementAPI.Application.DTOs.Product;

namespace InventoryManagementAPI.Application.Validators.Product;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del producto es requerido")
            .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(999999.99m).WithMessage("El precio no puede exceder S/. 999,999.99");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa")
            .LessThanOrEqualTo(999999).WithMessage("La cantidad no puede exceder 999,999 unidades");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("La categoría es requerida")
            .Length(2, 50).WithMessage("La categoría debe tener entre 2 y 50 caracteres");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del producto es requerido")
            .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(999999.99m).WithMessage("El precio no puede exceder S/. 999,999.99");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa")
            .LessThanOrEqualTo(999999).WithMessage("La cantidad no puede exceder 999,999 unidades");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("La categoría es requerida")
            .Length(2, 50).WithMessage("La categoría debe tener entre 2 y 50 caracteres");
    }
}
