namespace Fora.Infra.Security.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPasswordWithKey(string password);
    }
}