using Fora.Domain.Entities;
using Fora.Domain.Interfaces.Infra.Data;
using Fora.Infra.Data.Context;
using Fora.Infra.Data.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace Fora.Infra.Data.Repository;

public class CompanyRepository(ForaContext context, IUnitOfWork unitOfWork) : Repository<Company>(context, unitOfWork), ICompanyRepository
{
    public async Task InsertCompaniesFromImporterAsync(List<Company> companies)
    {
        await TruncateAllTablesAsync();

        await Insert(companies);
    }

    public async Task TruncateAllTablesAsync()
    {
        _unitOfWork.BeginTransaction();

        await Db.Database.ExecuteSqlRawAsync("DELETE FROM CompanyNetIncomeLoss");
        await Db.Database.ExecuteSqlRawAsync("DELETE FROM Company");

        await _unitOfWork.CommitAsync();
    }

    public async Task<List<Company>> GetCompaniesAsync(string? startsWithLetter = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(startsWithLetter))
        {
            var letter = startsWithLetter.Trim().ToUpper();
            query = query.Where(c => c.Name.ToUpper().StartsWith(letter));
        }

        var companies = await query.Include(x => x.CompanyNetIncomeLoss).ToListAsync(cancellationToken);

        return companies;
    }


}
