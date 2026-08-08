using FluentValidation;
using OrderManagement.API.DTOs.Authentication;

namespace OrderManagement.API.Validators
{
    public class ResetPasswordRequestValidator
        : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Token)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .MinimumLength(8);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword);
        }
    }
}