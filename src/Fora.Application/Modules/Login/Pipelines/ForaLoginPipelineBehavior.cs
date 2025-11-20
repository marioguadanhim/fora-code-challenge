using ErrorOr;
using Fora.Application.CommonViewModels.Base.Errors;
using Fora.Application.Modules.Login.Contracts;
using Fora.Domain.Interfaces.Services;
using MediatR;

namespace Fora.Application.Modules.Login.Pipelines;

public class ForaLoginPipelineBehavior(IForaAuthenticationService midasAuthenticationService) : IPipelineBehavior<ForaLoginRequest, ErrorOr<ForaLoginResponse>>
{
    private readonly IForaAuthenticationService _midasAuthenticationService = midasAuthenticationService;
    public async Task<ErrorOr<ForaLoginResponse>> Handle(ForaLoginRequest request, RequestHandlerDelegate<ErrorOr<ForaLoginResponse>> next, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _midasAuthenticationService.AuthenticateUserForLogin(request.UserName, request.Password);

            var response = new ForaLoginResponse
            {
                Token = token
            };

            return response;
        }
        catch (Exception ex)
        {
            var error = Error.Unauthorized(
              code: ErrorCodes.AuthenticationError,
              description: ex.Message
            );

            return ErrorOr<ForaLoginResponse>.From(new List<Error> { error });
        }
    }

}
