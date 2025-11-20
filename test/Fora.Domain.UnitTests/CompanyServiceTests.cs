using Fora.Domain.Entities;
using Fora.Domain.Interfaces.Infra.Data;
using Fora.Domain.Services;
using NSubstitute;

namespace Fora.Domain.UnitTests
{
    public class CompanyServiceTests
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly CompanyService _sut; // System Under Test

        public CompanyServiceTests()
        {
            _companyRepository = Substitute.For<ICompanyRepository>();
            _sut = new CompanyService(_companyRepository);
        }

        #region InsertCompaniesFromImporterAsync Tests

        [Fact]
        public async Task InsertCompaniesFromImporterAsync_ShouldCallRepository()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Cik = 1, Name = "Test Company" }
            };

            // Act
            await _sut.InsertCompaniesFromImporterAsync(companies);

            // Assert
            await _companyRepository.Received(1).InsertCompaniesFromImporterAsync(companies);
        }

        #endregion

        #region GetCompaniesWithFundingAsync Tests

        [Fact]
        public async Task GetCompaniesWithFundingAsync_WithNoFilter_ShouldReturnAllCompanies()
        {
            // Arrange
            var companies = CreateTestCompanies();
            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(companies);

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync(null);

            // Assert
            Assert.NotEmpty(result);
            await _companyRepository.Received(1).GetCompaniesAsync(null, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetCompaniesWithFundingAsync_WithFilter_ShouldPassFilterToRepository()
        {
            // Arrange
            var companies = CreateTestCompanies();
            _companyRepository.GetCompaniesAsync("A", Arg.Any<CancellationToken>())
                .Returns(companies);

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync("A");

            // Assert
            await _companyRepository.Received(1).GetCompaniesAsync("A", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetCompaniesWithFundingAsync_WithValidData_ShouldCalculateFunding()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                8_000_000_000,
                9_000_000_000);

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            Assert.Single(result);
            var (resultCompany, standardAmount, specialAmount) = result[0];
            Assert.Equal(company.Cik, resultCompany.Cik);
            Assert.True(standardAmount > 0);
            Assert.True(specialAmount > 0);
        }

        #endregion

        #region StandardFundableAmount Calculation Tests

        [Fact]
        public async Task StandardFundableAmount_WithMissingYears_ShouldReturnZero()
        {
            // Arrange - Missing 2020
            var company = CreateCompanyWithIncompleteYearlyData(123, "Test Company");
            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            Assert.Equal(0m, standardAmount);
            Assert.Equal(0m, specialAmount);
        }

        [Fact]
        public async Task StandardFundableAmount_WithNegativeIncome2021_ShouldReturnZero()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                -1_000_000_000, // Negative 2021
                9_000_000_000);

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, _) = result[0];
            Assert.Equal(0m, standardAmount);
        }

        [Fact]
        public async Task StandardFundableAmount_WithNegativeIncome2022_ShouldReturnZero()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                8_000_000_000,
                -1_000_000_000); // Negative 2022

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, _) = result[0];
            Assert.Equal(0m, standardAmount);
        }

        [Fact]
        public async Task StandardFundableAmount_WithIncomeAbove10Billion_ShouldApply1233Percent()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                11_000_000_000,
                12_000_000_000); // 12B is highest

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, _) = result[0];
            var expected = Math.Round(12_000_000_000m * 0.1233m, 2);
            Assert.Equal(expected, standardAmount);
        }

        [Fact]
        public async Task StandardFundableAmount_WithIncomeBelow10Billion_ShouldApply2151Percent()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                8_000_000_000,
                9_000_000_000); // 9B is highest, below 10B

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, _) = result[0];
            var expected = Math.Round(9_000_000_000m * 0.2151m, 2);
            Assert.Equal(expected, standardAmount);
        }

        [Fact]
        public async Task StandardFundableAmount_WithIncomeExactly10Billion_ShouldApply1233Percent()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                8_000_000_000,
                10_000_000_000); // Exactly 10B

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, _) = result[0];
            var expected = Math.Round(10_000_000_000m * 0.1233m, 2);
            Assert.Equal(expected, standardAmount);
        }

        #endregion

        #region SpecialFundableAmount Calculation Tests

        [Theory]
        [InlineData("Apple Inc")]
        [InlineData("Exxon Mobile")]
        [InlineData("Intel Corp")]
        [InlineData("Oracle")]
        [InlineData("Uber Technologies")]
        public async Task SpecialFundableAmount_WithVowelStart_ShouldAdd15Percent(string companyName)
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                companyName,
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                8_000_000_000,
                9_000_000_000);

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            var expected = Math.Round(standardAmount + (standardAmount * 0.15m), 2);
            Assert.Equal(expected, specialAmount);
        }

        [Theory]
        [InlineData("Microsoft")]
        [InlineData("Tesla")]
        [InlineData("Boeing")]
        public async Task SpecialFundableAmount_WithConsonantStart_ShouldNotAdd15Percent(string companyName)
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                companyName,
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                8_000_000_000,
                9_000_000_000);

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            // Should be equal since no vowel bonus
            Assert.Equal(standardAmount, specialAmount);
        }

        [Fact]
        public async Task SpecialFundableAmount_With2022IncomeLessThan2021_ShouldSubtract25Percent()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                9_000_000_000,
                8_000_000_000); // 2022 < 2021

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            var expected = Math.Round(standardAmount - (standardAmount * 0.25m), 2);
            Assert.Equal(expected, specialAmount);
        }

        [Fact]
        public async Task SpecialFundableAmount_WithVowelAndDecliningIncome_ShouldApplyBoth()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Apple Inc",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                9_000_000_000,
                8_000_000_000); // Vowel + Declining

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            var expected = standardAmount + (standardAmount * 0.15m) - (standardAmount * 0.25m);
            expected = Math.Round(expected, 2);
            Assert.Equal(expected, specialAmount);
        }

        [Fact]
        public async Task SpecialFundableAmount_With2022IncomeEqualTo2021_ShouldNotSubtract()
        {
            // Arrange
            var company = CreateCompanyWithCompleteYearlyData(
                123,
                "Test Company",
                5_000_000_000,
                6_000_000_000,
                7_000_000_000,
                9_000_000_000,
                9_000_000_000); // Equal

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            Assert.Equal(standardAmount, specialAmount);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public async Task GetCompaniesWithFundingAsync_WithEmptyNetIncomeLoss_ShouldReturnZeroFunding()
        {
            // Arrange
            var company = new Company
            {
                Cik = 123,
                Name = "Test Company",
                CompanyNetIncomeLoss = new List<CompanyNetIncomeLoss>() // Empty
            };

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            Assert.Equal(0m, standardAmount);
            Assert.Equal(0m, specialAmount);
        }

        [Fact]
        public async Task GetCompaniesWithFundingAsync_WithNullNetIncomeLoss_ShouldReturnZeroFunding()
        {
            // Arrange
            var company = new Company
            {
                Cik = 123,
                Name = "Test Company",
                CompanyNetIncomeLoss = null! // Null
            };

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, specialAmount) = result[0];
            Assert.Equal(0m, standardAmount);
            Assert.Equal(0m, specialAmount);
        }

        [Fact]
        public async Task GetCompaniesWithFundingAsync_WithDuplicateYears_ShouldUseMaxValue()
        {
            // Arrange
            var company = new Company
            {
                Cik = 123,
                Name = "Test Company",
                CompanyNetIncomeLoss = new List<CompanyNetIncomeLoss>
                {
                    CreateNetIncomeLoss(2018, 5_000_000_000),
                    CreateNetIncomeLoss(2019, 6_000_000_000),
                    CreateNetIncomeLoss(2020, 7_000_000_000),
                    CreateNetIncomeLoss(2021, 8_000_000_000),
                    CreateNetIncomeLoss(2022, 9_000_000_000),
                    CreateNetIncomeLoss(2022, 10_000_000_000), // Duplicate 2022 with higher value
                }
            };

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, _) = result[0];
            // Should use 10B (highest) for 2022
            var expected = Math.Round(10_000_000_000m * 0.1233m, 2);
            Assert.Equal(expected, standardAmount);
        }

        [Fact]
        public async Task GetCompaniesWithFundingAsync_WithNon10KForms_ShouldIgnoreThem()
        {
            // Arrange
            var company = new Company
            {
                Cik = 123,
                Name = "Test Company",
                CompanyNetIncomeLoss = new List<CompanyNetIncomeLoss>
                {
                    CreateNetIncomeLoss(2018, 5_000_000_000, "10-Q"), // Wrong form
                    CreateNetIncomeLoss(2019, 6_000_000_000),
                    CreateNetIncomeLoss(2020, 7_000_000_000),
                    CreateNetIncomeLoss(2021, 8_000_000_000),
                    CreateNetIncomeLoss(2022, 9_000_000_000),
                }
            };

            _companyRepository.GetCompaniesAsync(null, Arg.Any<CancellationToken>())
                .Returns(new List<Company> { company });

            // Act
            var result = await _sut.GetCompaniesWithFundingAsync();

            // Assert
            var (_, standardAmount, _) = result[0];
            // Should be 0 because 2018 is missing (10-Q ignored)
            Assert.Equal(0m, standardAmount);
        }

        #endregion

        #region Helper Methods

        private List<Company> CreateTestCompanies()
        {
            return new List<Company>
            {
                CreateCompanyWithCompleteYearlyData(
                    1,
                    "Company A",
                    5_000_000_000,
                    6_000_000_000,
                    7_000_000_000,
                    8_000_000_000,
                    9_000_000_000),
                CreateCompanyWithCompleteYearlyData(
                    2,
                    "Company B",
                    3_000_000_000,
                    4_000_000_000,
                    5_000_000_000,
                    6_000_000_000,
                    7_000_000_000)
            };
        }

        private Company CreateCompanyWithCompleteYearlyData(
            int cik,
            string name,
            long income2018,
            long income2019,
            long income2020,
            long income2021,
            long income2022)
        {
            return new Company
            {
                Cik = cik,
                Name = name,
                CompanyNetIncomeLoss = new List<CompanyNetIncomeLoss>
                {
                    CreateNetIncomeLoss(2018, income2018),
                    CreateNetIncomeLoss(2019, income2019),
                    CreateNetIncomeLoss(2020, income2020),
                    CreateNetIncomeLoss(2021, income2021),
                    CreateNetIncomeLoss(2022, income2022)
                }
            };
        }

        private Company CreateCompanyWithIncompleteYearlyData(int cik, string name)
        {
            return new Company
            {
                Cik = cik,
                Name = name,
                CompanyNetIncomeLoss = new List<CompanyNetIncomeLoss>
                {
                    CreateNetIncomeLoss(2018, 5_000_000_000),
                    CreateNetIncomeLoss(2019, 6_000_000_000),
                    // 2020 missing
                    CreateNetIncomeLoss(2021, 8_000_000_000),
                    CreateNetIncomeLoss(2022, 9_000_000_000)
                }
            };
        }

        private CompanyNetIncomeLoss CreateNetIncomeLoss(int year, long value, string form = "10-K")
        {
            return new CompanyNetIncomeLoss
            {
                CompanyNetIncomeLossId = Guid.NewGuid(),
                LossValue = value,
                LossFormat = form,
                LossFrame = $"CY{year}"
            };
        }

        #endregion
    }
}