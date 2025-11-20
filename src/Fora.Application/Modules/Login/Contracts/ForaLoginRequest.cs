using ErrorOr;
using Fora.Application.CommonViewModels.Base;
using MediatR;

namespace Fora.Application.Modules.Login.Contracts;

public class ForaLoginRequest : BaseRequest, IRequest<ErrorOr<ForaLoginResponse>>
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
