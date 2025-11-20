using Fora.Infra.CrossCutting;
using Fora.Infra.Data.Context;
using Fora.Worker.DataImporter;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();
                services.AddHostedService<ApiPollingService>();
                ForaContainer.Register(services);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddEventLog();
            })
            .Build();

        await ApplyMigrationsAsync(host.Services);

        await host.RunAsync();
    }

    private static async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Checking for pending database migrations...");

            var dbContext = scope.ServiceProvider.GetRequiredService<ForaContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Found {Count} pending migrations. Applying...", pendingMigrations.Count());

                await dbContext.Database.MigrateAsync();

                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date. No pending migrations.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw; // Re-throw to prevent the application from starting with an outdated database
        }
    }
}