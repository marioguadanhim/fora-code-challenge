namespace Fora.Application.Interfaces;

public interface IImporterApplication
{
    Task RunApiPooling(CancellationToken stoppingToken = default);
}