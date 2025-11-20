using Fora.Domain.Entities;
using Fora.Domain.Interfaces.Infra.Data;
using Fora.Domain.Interfaces.Services;
using Fora.Infra.Security.Interfaces;

namespace Fora.Domain.Services;

public class ForaAuthenticationService(IWebUserRepository webUserRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator) : IForaAuthenticationService
{
    private readonly IWebUserRepository _webUserRepository = webUserRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    public async Task<string> AuthenticateUserForLogin(string userName, string password)
    {
        string hashedPassword = _passwordHasher.HashPasswordWithKey(password);
        WebUser? webUser = await _webUserRepository.GetForLogin(userName, hashedPassword);
        if (webUser is null)
            throw new Exception("User or Password is wrong");

        return _jwtTokenGenerator.GenerateForLogin(webUser.UserName, webUser.Role);
    }
}
