using EcomAPI.DTOs.Orders;
using FluentValidation;

namespace EcomAPI.Validators;

public class CreateOrderItemRequestValidator
    : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}