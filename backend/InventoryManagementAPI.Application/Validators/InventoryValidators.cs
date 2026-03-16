using FluentValidation;
using InventoryManagementAPI.Application.DTOs.Inventory;

namespace InventoryManagementAPI.Application.Validators.Inventory;

/// <summary>
/// Validador para crear un movimiento de inventario
/// </summary>
public class CreateStockMovementRequestValidator : AbstractValidator<CreateStockMovementRequest>
{
    public CreateStockMovementRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("El ID del producto es requerido e inválido");

        RuleFor(x => x.MovementType)
            .NotEmpty().WithMessage("El tipo de movimiento es requerido")
            .Must(type => IsValidMovementType(type))
            .WithMessage("Tipo de movimiento inválido. Valores válidos: purchase, sale, adjustment, return, damage, transfer");


        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("La cantidad es requerida")
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(999999).WithMessage("La cantidad no puede exceder 999.999 unidades");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres");
    }

    private bool IsValidMovementType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;

        var validTypes = new[] { "purchase", "sale", "adjustment", "return", "damage", "transfer" };
        return validTypes.Contains(type.ToLower());
    }
}

/// <summary>
/// Validador para ajuste de inventario
/// </summary>
public class CreateInventoryAdjustmentRequestValidator : AbstractValidator<CreateInventoryAdjustmentRequest>
{
    public CreateInventoryAdjustmentRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("El ID del producto es requerido e inválido");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa")
            .LessThanOrEqualTo(999999).WithMessage("La cantidad no puede exceder 999.999 unidades");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo del ajuste es requerido")
            .Length(5, 500).WithMessage("El motivo debe tener entre 5 y 500 caracteres");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres");
    }
}

/// <summary>
/// Validador para actualizar niveles de reorden
/// </summary>
public class UpdateReorderLevelsRequestValidator : AbstractValidator<UpdateReorderLevelsRequest>
{
    public UpdateReorderLevelsRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("El ID del producto es requerido e inválido");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("El nivel de reorden no puede ser negativo")
            .LessThanOrEqualTo(100000).WithMessage("El nivel de reorden no puede exceder 100.000");

        RuleFor(x => x.ReorderQuantity)
            .GreaterThan(0).WithMessage("La cantidad de reorden debe ser mayor a 0")
            .LessThanOrEqualTo(100000).WithMessage("La cantidad de reorden no puede exceder 100.000")
            .GreaterThanOrEqualTo(x => x.ReorderLevel).WithMessage("La cantidad de reorden debe ser mayor o igual al nivel de reorden");
    }
}
