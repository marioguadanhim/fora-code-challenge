using Fora.Domain.Interfaces.Infra.ExternalCommunication;
using Fora.Domain.Services;
using Fora.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fora.Domain.UnitTests
{
    public class ImporterServiceTests
    {
        private readonly IEdgarApiService _edgarApiService;
        private readonly ILogger<ImporterService> _logger;
        private readonly ImporterService _sut;

        public ImporterServiceTests()
        {
            _edgarApiService = Substitute.For<IEdgarApiService>();
            _logger = Substitute.For<ILogger<ImporterService>>();

            _sut = new ImporterService(_edgarApiService, _logger);
        }

        [Fact]
        public async Task ImportAllCompaniesAsync_ShouldReturnCompanies_WhenApiReturnsValidData()
        {
            // Arrange
            _edgarApiService
                .GetCompanyFactsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(CreateValidCompany());

            // Act
            var result = await _sut.ImportAllCompaniesAsync();

            // Assert
            Assert.NotEmpty(result);
            await _edgarApiService.Received()
                .GetCompanyFactsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ImportAllCompaniesAsync_ShouldContinue_WhenExceptionOccurs()
        {
            // Arrange
            _edgarApiService
                .GetCompanyFactsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("API failure"));

            // Act
            var result = await _sut.ImportAllCompaniesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ImportAllCompaniesAsync_ShouldStop_WhenCancellationRequested()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await _sut.ImportAllCompaniesAsync(cts.Token);

            // Assert
            Assert.Empty(result);
            await _edgarApiService.DidNotReceive()
                .GetCompanyFactsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        private EdgarCompanyInfo CreateValidCompany()
        {
            return new EdgarCompanyInfo
            {
                Cik = 123456,
                EntityName = "Test Corp",
                Facts = new EdgarCompanyInfo.InfoFact
                {
                    UsGaap = new EdgarCompanyInfo.InfoFactUsGaap
                    {
                        NetIncomeLoss = new EdgarCompanyInfo.InfoFactUsGaapNetIncomeLoss
                        {
                            Units = new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnits
                            {
                                Usd = new[]
                                {
                                    new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd
                                    {
                                        Form = "10-K",
                                        Frame = "CY2022",
                                        Val = 1_000_000m
                                    },
                                    new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd
                                    {
                                        Form = "10-K",
                                        Frame = "CY2021",
                                        Val = 800_000m
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
