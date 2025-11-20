using AutoMapper;
using Fora.Domain.Entities;
using Fora.Domain.ValueObjects;

namespace Fora.Application.AutoMapper;

public class DomainToDomainMapper : Profile
{
    public DomainToDomainMapper()
    {
        CreateMap<EdgarCompanyInfo, Company>()
                .ForMember(dest => dest.Cik, opt => opt.MapFrom(src => src.Cik))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EntityName))
                .ForMember(dest => dest.CompanyNetIncomeLoss, opt => opt.MapFrom(src =>
                    src.Facts.UsGaap.NetIncomeLoss.Units.Usd
                        .Where(x => x.Form == "10-K" &&
                                    !string.IsNullOrEmpty(x.Frame) &&
                                    x.Frame.StartsWith("CY") &&
                                    x.Frame.Length == 6)
                        .Select(x => new CompanyNetIncomeLoss
                        {
                            CompanyNetIncomeLossId = Guid.NewGuid(),
                            Cik = src.Cik,
                            LossValue = (long)x.Val,
                            LossFormat = x.Form,
                            LossFrame = x.Frame
                        })
                        .ToList()));
    }
}
