using Fora.Domain.Entities;
using Fora.Domain.Interfaces.Infra.Data;
using Fora.Domain.Interfaces.Services;

namespace Fora.Domain.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task InsertCompaniesFromImporterAsync(List<Company> companies)
    {
        await _companyRepository.InsertCompaniesFromImporterAsync(companies);
    }

    public async Task<List<(Company Company, decimal StandardAmount, decimal SpecialAmount)>> GetCompaniesWithFundingAsync(string? startsWithLetter = null,
        CancellationToken cancellationToken = default)
    {
        var companies = await _companyRepository.GetCompaniesAsync(startsWithLetter, cancellationToken);

        var results = new List<(Company Company, decimal StandardAmount, decimal SpecialAmount)>();

        foreach (var company in companies)
        {
            var yearlyIncomes = ExtractYearlyIncomes(company);
            var standardAmount = CalculateStandardFundableAmount(company.Name, yearlyIncomes);
            var specialAmount = CalculateSpecialFundableAmount(company.Name, yearlyIncomes, standardAmount);

            results.Add((company, Math.Round(standardAmount, 2), Math.Round(specialAmount, 2)));
        }

        return results;
    }

    private Dictionary<int, decimal> ExtractYearlyIncomes(Company company)
    {
        var yearlyIncomes = new Dictionary<int, decimal>();


        if (company.CompanyNetIncomeLoss == null || !company.CompanyNetIncomeLoss.Any())
        {
            return yearlyIncomes;
        }

        var yearlyData = company.CompanyNetIncomeLoss
            .Where(d => d.LossFormat == "10-K" &&
                        !string.IsNullOrEmpty(d.LossFrame) &&
                        d.LossFrame.StartsWith("CY") &&
                        d.LossFrame.Length == 6)
            .ToList();

        foreach (var data in yearlyData)
        {
            if (int.TryParse(data.LossFrame.Substring(2), out int year))
            {
                if (!yearlyIncomes.ContainsKey(year))
                {
                    yearlyIncomes[year] = data.LossValue;
                }
                else
                {
                    yearlyIncomes[year] = Math.Max(yearlyIncomes[year], data.LossValue);
                }
            }
        }


        return yearlyIncomes;
    }

    private decimal CalculateStandardFundableAmount(string companyName, Dictionary<int, decimal> yearlyIncomes)
    {
        if (!yearlyIncomes.ContainsKey(2018) || !yearlyIncomes.ContainsKey(2019) ||
            !yearlyIncomes.ContainsKey(2020) || !yearlyIncomes.ContainsKey(2021) ||
            !yearlyIncomes.ContainsKey(2022))
        {
            return 0m;
        }

        var income2021 = yearlyIncomes[2021];
        var income2022 = yearlyIncomes[2022];

        if (income2021 <= 0 || income2022 <= 0)
        {
            return 0m;
        }

        var highestIncome = new[]
        {
                yearlyIncomes[2018],
                yearlyIncomes[2019],
                yearlyIncomes[2020],
                yearlyIncomes[2021],
                yearlyIncomes[2022]
            }.Max();

        const decimal tenBillion = 10_000_000_000m;
        const decimal highIncomePercentage = 0.1233m; 
        const decimal lowIncomePercentage = 0.2151m; 

        if (highestIncome >= tenBillion)
        {
            return highestIncome * highIncomePercentage;
        }
        else
        {
            return highestIncome * lowIncomePercentage;
        }
    }

    private decimal CalculateSpecialFundableAmount(
        string companyName,
        Dictionary<int, decimal> yearlyIncomes,
        decimal standardAmount)
    {
        var specialAmount = standardAmount;

        if (!string.IsNullOrEmpty(companyName))
        {
            var firstChar = char.ToUpper(companyName[0]);
            if (firstChar == 'A' || firstChar == 'E' || firstChar == 'I' ||
                firstChar == 'O' || firstChar == 'U')
            {
                specialAmount += standardAmount * 0.15m;
            }
        }

        if (yearlyIncomes.ContainsKey(2022) && yearlyIncomes.ContainsKey(2021))
        {
            var income2022 = yearlyIncomes[2022];
            var income2021 = yearlyIncomes[2021];

            if (income2022 < income2021)
            {
                specialAmount -= standardAmount * 0.25m;
            }
        }

        return specialAmount;
    }
}
