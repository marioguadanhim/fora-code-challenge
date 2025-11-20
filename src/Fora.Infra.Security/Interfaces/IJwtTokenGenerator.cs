namespace Fora.Infra.Security.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateForLogin(string userName, List<string> roles);
        string GenerateForLogin(string userName, params string[] roles);
    }
}