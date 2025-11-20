using AutoMapper;
using Fora.Application.Modules.Companies.Contracts;
using Fora.Domain.Entities;

namespace Fora.Application.AutoMapper;

public class DomainToModelMapper : Profile
{
    public DomainToModelMapper()
    {
        CreateMap<(Company Company, decimal StandardAmount, decimal SpecialAmount), GetCompaniesFundingItemResponse>()
                       .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Company.Cik))
                       .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Company.Name))
                       .ForMember(dest => dest.StandardFundableAmount, opt => opt.MapFrom(src => src.StandardAmount))
                       .ForMember(dest => dest.SpecialFundableAmount, opt => opt.MapFrom(src => src.SpecialAmount));

        CreateMap<List<(Company Company, decimal StandardAmount, decimal SpecialAmount)>, GetCompaniesFundingResponse>()
            .ForMember(dest => dest.GetCompaniesFundingItemResponse,
                opt => opt.MapFrom(src => src));
    }
}
