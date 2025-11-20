using Fora.Domain.ValueObjects;

namespace Fora.Domain.Interfaces.Services
{
    public interface IImporterService
    {
        Task<List<EdgarCompanyInfo>> ImportAllCompaniesAsync(CancellationToken stoppingToken = default);
    }
}