using Fora.Domain.Entities;

namespace Fora.Domain.Interfaces.Infra.Data;

public interface ICompanyRepository : IRepository<Company>
{
    Task<List<Company>> GetCompaniesAsync(string? startsWithLetter = null, CancellationToken cancellationToken = default);
    Task InsertCompaniesFromImporterAsync(List<Company> companies);
}
