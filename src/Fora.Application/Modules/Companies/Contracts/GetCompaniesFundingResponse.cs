
namespace Fora.Application.Modules.Companies.Contracts;

public class GetCompaniesFundingResponse
{
    public List<GetCompaniesFundingItemResponse> GetCompaniesFundingItemResponse { get; set; } = [];
}

public class GetCompaniesFundingItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal StandardFundableAmount { get; set; }
    public decimal SpecialFundableAmount { get; set; }
}