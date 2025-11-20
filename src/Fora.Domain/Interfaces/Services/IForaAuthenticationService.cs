namespace Fora.Domain.Interfaces.Services;

public interface IForaAuthenticationService
{
    Task<string> AuthenticateUserForLogin(string userName, string password);
}