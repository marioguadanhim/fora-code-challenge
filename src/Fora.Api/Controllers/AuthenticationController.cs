using Fora.Api.Controllers.Base;
using Fora.Application.Modules.Login.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Fora.Api.Controllers;

public class AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<AuthenticationController> _logger = logger;

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] ForaLoginRequest loginUser)
    {
        var result = await _mediator.Send(loginUser);
        return HandleResult(result);
    }
}
