using Fora.Domain.ValueObjects;

namespace Fora.Domain.Interfaces.Infra.ExternalCommunication
{
    public interface IEdgarApiService
    {
        Task<EdgarCompanyInfo?> GetCompanyFactsAsync(int cik, CancellationToken cancellationToken = default);
    }
}
