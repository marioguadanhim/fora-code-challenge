using Fora.Domain.Entities;

namespace Fora.Domain.Interfaces.Infra.Data;

public interface IWebUserRepository
{
    Task<WebUser?> GetForLogin(string userName, string hashedPassword);
}
