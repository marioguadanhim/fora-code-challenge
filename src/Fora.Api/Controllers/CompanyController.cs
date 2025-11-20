using Fora.Api.Controllers.Base;
using Fora.Application.Modules.Companies.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fora.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CompanyController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(GetCompaniesFundingResponse), 200)]
    public async Task<IActionResult> GetCompaniesWithFundingAsync([FromQuery] string? startsWithLetter = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetCompaniesFundingRequest(startsWithLetter), cancellationToken);
        return HandleResult(response);
    }
}
