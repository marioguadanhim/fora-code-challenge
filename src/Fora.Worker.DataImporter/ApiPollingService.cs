using Fora.Application.Interfaces;

namespace Fora.Worker.DataImporter
{
    public class ApiPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ApiPollingService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

        public ApiPollingService(IServiceScopeFactory scopeFactory, ILogger<ApiPollingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Importer Api Polling Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var importerApplication = scope.ServiceProvider.GetRequiredService<IImporterApplication>();

                    try
                    {
                        await importerApplication.RunApiPooling(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while checking for new data.");
                    }
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }

            _logger.LogInformation("Importer Api Polling Service is stopping.");
        }


    }
}
