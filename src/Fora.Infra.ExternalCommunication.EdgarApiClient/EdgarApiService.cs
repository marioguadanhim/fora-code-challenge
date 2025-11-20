using Fora.Domain.Interfaces.Infra.ExternalCommunication;
using Fora.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Text.Json;

namespace Fora.Infra.ExternalCommunication.EdgarApiClient
{
    public class EdgarApiService : IEdgarApiService
    {
        private readonly RestClient _restClient;
        private readonly ILogger<EdgarApiService> _logger;
        private const string BaseUrl = "https://data.sec.gov/api/xbrl/companyfacts";

        public EdgarApiService(ILogger<EdgarApiService> logger)
        {
            _logger = logger;

            var options = new RestClientOptions(BaseUrl)
            {
                MaxTimeout = 30000,            
                UserAgent = "mario.a.guadagnin@gmail.com"
            };

            _restClient = new RestClient(options);
        }

        public async Task<EdgarCompanyInfo?> GetCompanyFactsAsync(int cik, CancellationToken cancellationToken = default)
        {
            try
            {
                var cikFormatted = cik.ToString("D10");
                var resourcePath = $"CIK{cikFormatted}.json";

                _logger.LogInformation("Fetching data for CIK: {Cik} from {Url}", cik, $"{BaseUrl}/{resourcePath}");

                var request = new RestRequest(resourcePath, Method.Get);

                var response = await _restClient.ExecuteAsync(request, cancellationToken);

                if (!response.IsSuccessful)
                {
                    _logger.LogWarning("Failed to fetch data for CIK: {Cik}. Status: {StatusCode}, Error: {ErrorMessage}",
                        cik, response.StatusCode, response.ErrorMessage);
                    return null;
                }

                if (string.IsNullOrEmpty(response.Content))
                {
                    _logger.LogWarning("Empty response for CIK: {Cik}", cik);
                    return null;
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var companyInfo = JsonSerializer.Deserialize<EdgarCompanyInfo>(response.Content, jsonOptions);

                if (companyInfo != null)
                {
                    _logger.LogInformation("Successfully fetched data for CIK: {Cik}, Entity: {EntityName}",
                        cik, companyInfo.EntityName);
                }

                return companyInfo;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for CIK: {Cik}", cik);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching data for CIK: {Cik}", cik);
                return null;
            }
        }

        public void Dispose()
        {
            _restClient?.Dispose();
        }
    }
}