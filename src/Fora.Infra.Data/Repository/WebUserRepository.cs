using Fora.Domain.Entities;
using Fora.Domain.Interfaces.Infra.Data;
using Fora.Infra.Data.Context;
using Fora.Infra.Data.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace Fora.Infra.Data.Repository;

public class WebUserRepository(ForaContext context, IUnitOfWork unitOfWork) : Repository<WebUser>(context, unitOfWork), IWebUserRepository
{
    public async Task<WebUser?> GetForLogin(string userName, string hashedPassword)
    {
        WebUser? webUser = await DbSet
            .Where(_ => _.UserName == userName && _.Password == hashedPassword)
            .FirstOrDefaultAsync();

        return webUser;
    }
}