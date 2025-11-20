using AutoMapper;
using Fora.Application.Interfaces;
using Fora.Domain.Entities;
using Fora.Domain.Interfaces.Services;
using Fora.Domain.ValueObjects;

namespace Fora.Application.Applications;

public class ImporterApplication : IImporterApplication
{
    private readonly IImporterService _importerService;
    private readonly ICompanyService _companyService;
    private readonly IMapper _mapper;

    public ImporterApplication(
        IImporterService importerService,
        ICompanyService companyService,
        IMapper mapper
        )
    {
        _importerService = importerService;
        _companyService = companyService;
        _mapper = mapper;
    }

    public async Task RunApiPooling(CancellationToken stoppingToken = default)
    {
        List<EdgarCompanyInfo> edgarCompanyInfos = await _importerService.ImportAllCompaniesAsync(stoppingToken);

        var companies = _mapper.Map<List<Company>>(edgarCompanyInfos);

        await _companyService.InsertCompaniesFromImporterAsync(companies);
    }
}
