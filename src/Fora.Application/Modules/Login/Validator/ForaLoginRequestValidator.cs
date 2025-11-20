using FluentValidation;
using Fora.Application.Modules.Login.Contracts;

namespace Fora.Application.Modules.Login.Validator;

public class ForaLoginRequestValidator : AbstractValidator<ForaLoginRequest>
{
    public ForaLoginRequestValidator()
    {
        RuleFor(x => x.UserName)
          .NotEmpty().WithMessage("UserName cannot be empty.")
          .NotNull().WithMessage("UserName cannot be null.")
          .MaximumLength(200).WithMessage("UserName Maximum size is 200")
          .MinimumLength(3).WithMessage("UserName Minimum size is 3.");

        RuleFor(x => x.Password)
           .NotEmpty().WithMessage("Password cannot be empty.")
           .NotNull().WithMessage("Password cannot be null.")
           .MaximumLength(200).WithMessage("Password Maximum size is 200")
           .MinimumLength(4).WithMessage("Password Minimum size is 4.");
    }
}
