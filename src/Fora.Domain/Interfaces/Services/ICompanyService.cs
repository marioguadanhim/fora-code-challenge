using Fora.Domain.Entities;

namespace Fora.Domain.Interfaces.Services;

public interface ICompanyService
{
    Task<List<(Company Company, decimal StandardAmount, decimal SpecialAmount)>> GetCompaniesWithFundingAsync(string? startsWithLetter = null, CancellationToken cancellationToken = default);
    Task InsertCompaniesFromImporterAsync(List<Company> companies);
}