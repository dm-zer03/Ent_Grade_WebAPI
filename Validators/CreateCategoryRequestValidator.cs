using EcomAPI.DTOs.Categories;
using FluentValidation;

namespace EcomAPI.Validators;

public class CreateCategoryRequestValidator
    : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}