using AutoMapper;
using ErrorOr;
using Fora.Application.CommonViewModels.Base.Errors;
using Fora.Application.Modules.Companies.Contracts;
using Fora.Domain.Interfaces.Services;
using MediatR;

namespace Fora.Application.Modules.Companies.Pipelines;

public class GetCompaniesFundingPipelineBehavior(ICompanyService companyService, IMapper mapper) : IPipelineBehavior<GetCompaniesFundingRequest, ErrorOr<GetCompaniesFundingResponse>>
{
    private readonly ICompanyService _companyService = companyService;
    private readonly IMapper _mapper = mapper;


    public async Task<ErrorOr<GetCompaniesFundingResponse>> Handle(GetCompaniesFundingRequest request, RequestHandlerDelegate<ErrorOr<GetCompaniesFundingResponse>> next, CancellationToken cancellationToken)
    {
        try
        {
            var companiesWithFunding = await _companyService.GetCompaniesWithFundingAsync(request.StartsWithLetter, cancellationToken);

            var response = _mapper.Map<GetCompaniesFundingResponse>(companiesWithFunding);

            return response;
        }
        catch (Exception ex)
        {
            var error = Error.Unauthorized(
              code: ErrorCodes.GetFundingCompanies,
              description: ex.Message
            );

            return ErrorOr<GetCompaniesFundingResponse>.From(new List<Error> { error });
        }
    }
}
