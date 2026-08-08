using FluentValidation;
using OrderManagement.API.DTOs.Authentication;

namespace OrderManagement.API.Validators
{
    public class ChangePasswordRequestValidator
        : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .MinimumLength(8);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword);
        }
    }
}