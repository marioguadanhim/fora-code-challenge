using Fora.Domain.Interfaces.Infra.ExternalCommunication;
using Fora.Domain.Interfaces.Services;
using Fora.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Fora.Domain.Services
{
    public class ImporterService : IImporterService
    {
        private readonly IEdgarApiService _edgarApiService;
        private readonly ILogger<ImporterService> _logger;

        private const int MaxRequestsPerSecond = 5; 
        private const int DelayBetweenRequestsMs = 1000 / MaxRequestsPerSecond; // 200ms between requests

        // CIKs from the challenge document
        private readonly int[] _ciks = new[]
        {
            18926, 892553, 1510524, 1858912, 1828248, 1819493, 60086, 1853630, 1761312, 1851182,
            1034665, 927628, 1125259, 1547660, 1393311, 1757143, 1958217, 312070, 310522, 1861841,
            1037868, 1696355, 1166834, 915912, 1085277, 831259, 882291, 1521036, 1824502, 1015647,
            884624, 1501103, 1397183, 1552797, 1894630, 823277, 21175, 1439124, 52827, 1730773,
            1867287, 1685428, 1007587, 92103, 1641751, 6845, 1231457, 947263, 895421, 1988979,
            1848898, 844790, 1541309, 1858007, 1729944, 726958, 1691221, 730272, 1308106, 884144,
            1108134, 1849058, 1435617, 1857518, 64803, 1912498, 1447380, 1232384, 1141788, 1549922,
            914475, 1498382, 1400897, 314808, 1323885, 1526520, 1550695, 1634293, 1756708, 1540159,
            1076691, 1980088, 1532346, 923796, 1849635, 1872292, 1227857, 1046311, 1710350, 1476150,
            1844642, 1967078, 14272, 933267, 1157557, 1560293, 217410, 1798562, 1038074, 1843370
        };

        public ImporterService(
            IEdgarApiService edgarApiService,
            ILogger<ImporterService> logger)
        {
            _edgarApiService = edgarApiService;
            _logger = logger;
        }

        public async Task<List<EdgarCompanyInfo>> ImportAllCompaniesAsync(CancellationToken stoppingToken = default)
        {
            var edgarCompanyInfos = new ConcurrentBag<EdgarCompanyInfo>();
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("Starting import of {Count} companies at {RequestsPerSecond} requests per second",
                _ciks.Length, MaxRequestsPerSecond);

            var successCount = 0;
            var failureCount = 0;

            for (int i = 0; i < _ciks.Length; i++)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Import cancelled after processing {Count} companies", i);
                    break;
                }

                var cik = _ciks[i];

                try
                {
                    _logger.LogInformation("Processing CIK {Current}/{Total}: {Cik}",
                        i + 1, _ciks.Length, cik);

                    var companyInfo = await _edgarApiService.GetCompanyFactsAsync(cik, stoppingToken);

                    if (companyInfo == null)
                    {
                        _logger.LogWarning("No data received for CIK: {Cik}", cik);
                        failureCount++;
                    }
                    else
                    {
                        var yearlyIncomes = ExtractYearlyIncomes(companyInfo);

                        if (yearlyIncomes.Count == 0)
                        {
                            _logger.LogWarning("No yearly income data found for CIK: {Cik} - {Name}",
                                cik, companyInfo.EntityName);
                        }

                        edgarCompanyInfos.Add(companyInfo);
                        successCount++;

                        _logger.LogInformation("Successfully imported CIK: {Cik} - {Name} with {Count} years of data",
                            cik, companyInfo.EntityName, yearlyIncomes.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing data for CIK: {Cik}", cik);
                    failureCount++;
                }

                if (i < _ciks.Length - 1)
                {
                    await Task.Delay(DelayBetweenRequestsMs, stoppingToken);
                }
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "Completed import in {ElapsedSeconds:F2} seconds - Success: {SuccessCount}, Failed: {FailureCount}, Total: {TotalCount}",
                stopwatch.Elapsed.TotalSeconds, successCount, failureCount, _ciks.Length);

            return edgarCompanyInfos.ToList();
        }

        private Dictionary<int, decimal> ExtractYearlyIncomes(EdgarCompanyInfo companyInfo)
        {
            var yearlyIncomes = new Dictionary<int, decimal>();

            if (companyInfo.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd == null)
            {
                return yearlyIncomes;
            }

            var relevantData = companyInfo.Facts.UsGaap.NetIncomeLoss.Units.Usd
                .Where(x => x.Form == "10-K" &&
                           !string.IsNullOrEmpty(x.Frame) &&
                           x.Frame.StartsWith("CY") &&
                           x.Frame.Length == 6)
                .ToList();

            foreach (var data in relevantData)
            {
                if (int.TryParse(data.Frame.Substring(2), out int year))
                {
                    if (!yearlyIncomes.ContainsKey(year) || yearlyIncomes[year] < data.Val)
                    {
                        yearlyIncomes[year] = data.Val;
                    }
                }
            }

            return yearlyIncomes;
        }
    }
}