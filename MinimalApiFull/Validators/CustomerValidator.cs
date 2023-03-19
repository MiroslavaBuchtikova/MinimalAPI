using FluentValidation;
using MinimalApiFull.Dtos;

namespace MinimalApiFull.Validators;

public class CustomerValidator: AbstractValidator<CustomerDto>
{
    public CustomerValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name cannot be empty");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name cannot be empty");
        RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Email address is invalid");
    }
}