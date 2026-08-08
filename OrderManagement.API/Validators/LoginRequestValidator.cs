using FluentValidation;
using OrderManagement.API.DTOs.Authentication;

namespace OrderManagement.API.Validators
{
    public class LoginRequestValidator
        : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            // RuleFor(x => x.Password)
            //  .NotEmpty();
            RuleFor(x => x.Password)
  .NotEmpty()
  .MinimumLength(8)
  .WithMessage("Password must be at least 8 characters.")
  .Matches("[A-Z]")
  .WithMessage("Password must contain one uppercase letter.")
  .Matches("[a-z]")
  .WithMessage("Password must contain one lowercase letter.")
  .Matches("[0-9]")
  .WithMessage("Password must contain one number.")
  .Matches("[^a-zA-Z0-9]")
  .WithMessage("Password must contain one special character.");
        }
    }
}