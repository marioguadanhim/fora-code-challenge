using ErrorOr;
using Fora.Application.CommonViewModels.Base;
using MediatR;

namespace Fora.Application.Modules.Companies.Contracts;

public class GetCompaniesFundingRequest : BaseRequest, IRequest<ErrorOr<GetCompaniesFundingResponse>>
{
    public GetCompaniesFundingRequest(string? startsWithLetter)
    {
        StartsWithLetter = startsWithLetter;
    }

    public string? StartsWithLetter { get; set; }
}
