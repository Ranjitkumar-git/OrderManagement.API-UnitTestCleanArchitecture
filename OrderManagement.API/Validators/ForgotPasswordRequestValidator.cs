using FluentValidation;
using OrderManagement.API.DTOs.Authentication;

namespace OrderManagement.API.Validators
{
    public class ForgotPasswordRequestValidator
        : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}