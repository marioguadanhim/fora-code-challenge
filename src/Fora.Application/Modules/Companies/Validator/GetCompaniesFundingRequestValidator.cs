using FluentValidation;
using Fora.Application.Modules.Companies.Contracts;

namespace Fora.Application.Modules.Companies.Validator;

public class GetCompaniesFundingRequestValidator : AbstractValidator<GetCompaniesFundingRequest>
{
    public GetCompaniesFundingRequestValidator()
    {
        RuleFor(x => x.StartsWithLetter)
          .MaximumLength(200).WithMessage("StartsWithLetter Maximum size is 200")
          .MinimumLength(1).WithMessage("StartsWithLetter Minimum size is 3.");
    }
}

